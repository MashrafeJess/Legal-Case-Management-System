using System.Diagnostics.Metrics;
using System.Security.Claims;
using System.Text.Json;
using AIT.Packages.SSLCommerz;
using AIT.Packages.SSLCommerz.Payload;
using AIT.Packages.SSLCommerz.Utils;
using Business.Settings;
using Database.Context;
using Database.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static Business.DTO.Payment.PaymentDto;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace Business.Services
{
    public class PaymentService(
        LMSContext context,
        IHttpContextAccessor accessor,
        IOptions<SSLCommerzSettings> settings,
        ILogger<PaymentService> logger)
    {
        private readonly LMSContext _context = context;
        private readonly IHttpContextAccessor _accessor = accessor;
        private readonly SSLCommerzSettings _settings = settings.Value;
        private readonly ILogger<PaymentService> _logger = logger;

        // ─── Initiate Payment ───────────────────────────────────────────
        public async Task<Result> InitiatePaymentAsync(InitiatePaymentDto dto)
        {
            var userId = _accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if case exists
            var caseExists = await _context.Case.AnyAsync(c => c.CaseId == dto.CaseId && !c.IsDeleted);
            if (!caseExists)
                return new Result(false, "Case not found");

            // Check if payment method exists and is active
            var method = await _context.PaymentMethod
                .FirstOrDefaultAsync(p => p.PaymentMethodId == dto.PaymentMethodId && p.PaymentStatus);
            if (method == null)
                return new Result(false, "Payment method not found or inactive");

            // Create PENDING payment record
            var payment = new Payment
            {
                Amount = dto.Amount,
                PaymentMethodId = dto.PaymentMethodId,
                CaseId = dto.CaseId,
                HearingId = dto.HearingId,
                Status = "PENDING",
                CreatedBy = userId
            };
            payment.TransactionId = payment.PaymentId;
            _context.Payment.Add(payment);
            var saveResult = await Result.DBCommitAsync(
                _context, "Payment initiated", _logger, "Failed to create payment", payment);

            if (!saveResult.Success) return saveResult;

            // Build SSLCommerz payload
            var payload = new SSLCommerzCreatePaymentPayload
            {
                StoreId = _settings.StoreId,
                StorePassword = _settings.StorePassword,
                Amount = dto.Amount,
                Currency = "BDT",
                TransactionId = payment.PaymentId,
                SuccessUrl = _settings.SuccessUrl,
                FailedUrl = _settings.FailUrl,
                CancelUrl = _settings.CancelUrl,
                IpnCallbackUrl = _settings.IpnUrl,
                CustomerName = dto.CustomerName,
                CustomerEmail = dto.CustomerEmail,
                CustomerPhone = dto.CustomerPhone,
                CustomerAddress = dto.CustomerAddress,
                ProductName = $"Case #{dto.CaseId} Payment",
                ProductCategory = "Legal Service",
            };

            var environment = _settings.IsSandbox
                ? SSLCommerzPaymentEnvironment.SANDBOX
                : SSLCommerzPaymentEnvironment.LIVE;

            var response = await AITSSLCommerzClient.CreatePaymentRequestAsync(payload, environment);
            _logger.LogInformation("SSLCommerz Response — IsSuccess: {IsSuccess}, PaymentUrl: {PaymentUrl}, RawData: {Data}",
    response.IsSuccess,
    response.Data?.PaymentUrl,
    System.Text.Json.JsonSerializer.Serialize(response.Data));

            if (!response.IsSuccess)
                return new Result(false, "Failed to connect to SSLCommerz");

            return new Result(true, "Payment initiated", new
            {
                payment.PaymentId,
                response?.Data?.PaymentUrl
            });
        }

        // ─── Success Callback ───────────────────────────────────────────
        public async Task<Result> PaymentSuccessAsync(string tranId, string valId)
        {
            _logger.LogInformation("Success callback — TranId: {TranId}, ValId: {ValId}",
                tranId, valId);

            // ✅ Only validate in production — sandbox val_id expires too fast
            if (!_settings.IsSandbox)
            {
                const string validationUrl =
                    "https://securepay.sslcommerz.com/validator/api/validationserverAPI.php" +
                    "?val_id={valId}" +
                    "&store_id={_settings.StoreId}" +
                    "&store_passwd={_settings.StorePassword}" +
                    "&v=1&format=json";

                try
                {
                    using var httpClient = new HttpClient();
                    var response = await httpClient.GetStringAsync(validationUrl);
                    var validation = JsonSerializer.Deserialize<JsonElement>(response);
                    var status = validation.GetProperty("status").GetString();

                    _logger.LogInformation("SSLCommerz validation status: {Status}", status);

                    if (status != "VALID" && status != "VALIDATED")
                        return new Result(false, $"Validation failed: {status}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Validation call failed");
                    return new Result(false, "Could not validate payment");
                }
            }
            else
            {
                // Sandbox — just log and skip validation
                _logger.LogInformation("Sandbox mode — skipping SSLCommerz validation");
            }

            // Find payment
            var payment = await _context.Payment
                .FirstOrDefaultAsync(p => p.TransactionId == tranId);

            _logger.LogInformation("Payment lookup — Found: {Found}", payment != null);

            if (payment == null)
                return new Result(false, "Payment not found");

            payment.Status = "SUCCESS";
            payment.ValidationId = valId;
            _context.Payment.Update(payment);

            if (payment.HearingId == null)
            {
                var caseEntity = await _context.Case
                    .FirstOrDefaultAsync(c => c.CaseId == payment.CaseId);
                if (caseEntity != null)
                {
                    caseEntity.IsConsultationFeePaid = true;
                    _context.Case.Update(caseEntity);
                }
            }
            else
            {
                var hearing = await _context.Hearing
                    .FirstOrDefaultAsync(h => h.HearingID == payment.HearingId);
                if (hearing != null)
                {
                    hearing.IsPaid = true;
                    hearing.UpdatedDate = DateTime.UtcNow;
                    _context.Hearing.Update(hearing);
                }
            }

            return await Result.DBCommitAsync(_context, "Payment successful", _logger);
        }

        // ─── Fail Callback ──────────────────────────────────────────────
        public async Task<Result> PaymentFailAsync(string tranId)
        {
            var payment = await _context.Payment
                .FirstOrDefaultAsync(p => p.PaymentId == tranId && !p.IsDeleted);

            if (payment == null)
                return new Result(false, "Payment record not found");

            payment.Status = "FAILED";
            payment.UpdatedDate = DateTime.UtcNow;
            payment.UpdatedBy = "SYSTEM";

            _context.Payment.Update(payment);
            return await Result.DBCommitAsync(_context, "Payment marked as failed", _logger);
        }

        // ─── Cancel Callback ────────────────────────────────────────────
        public async Task<Result> PaymentCancelAsync(string tranId)
        {
            var payment = await _context.Payment
                .FirstOrDefaultAsync(p => p.PaymentId == tranId && !p.IsDeleted);

            if (payment == null)
                return new Result(false, "Payment record not found");

            payment.Status = "CANCELLED";
            payment.UpdatedDate = DateTime.UtcNow;
            payment.UpdatedBy = "SYSTEM";

            _context.Payment.Update(payment);
            return await Result.DBCommitAsync(_context, "Payment cancelled", _logger);
        }

        // ─── Get Payments by Case ───────────────────────────────────────
        public async Task<Result> GetPaymentsByCaseAsync(int caseId)
        {
            var payments = await _context.Payment
                .Where(p => p.CaseId == caseId && !p.IsDeleted)
                .Select(p => new PaymentResponseDto
                {
                    PaymentId = p.PaymentId,
                    CaseId = p.CaseId,
                    Amount = p.Amount,
                    Status = p.Status!,
                    TransactionId = p.TransactionId,
                    ValidationId = p.ValidationId,
                    PaymentMethodName = p.Method!.PaymentMethodName,
                    CreatedBy = _context.User
                        .Where(u => u.UserId == p.CreatedBy)
                        .Select(u => u.UserName)
                        .FirstOrDefault() ?? "Unknown",
                    CreatedDate = (DateTime)p.CreatedDate!
                })
                .AsNoTracking()
                .ToListAsync();

            return payments.Count > 0
                ? new Result(true, "Payments retrieved", payments)
                : new Result(false, "No payments found");
        }

        // ─── Get Single Payment ─────────────────────────────────────────
        public async Task<Result> GetPaymentByIdAsync(string paymentId)
        {
            var payment = await _context.Payment
                .Where(p => p.PaymentId == paymentId && !p.IsDeleted)
                .Select(p => new PaymentResponseDto
                {
                    PaymentId = p.PaymentId,
                    CaseId = p.CaseId,
                    Amount = p.Amount,
                    Status = p.Status!,
                    TransactionId = p.TransactionId,
                    ValidationId = p.ValidationId,
                    PaymentMethodName = p.Method!.PaymentMethodName,
                    CreatedBy = _context.User
                        .Where(u => u.UserId == p.CreatedBy)
                        .Select(u => u.UserName)
                        .FirstOrDefault() ?? "Unknown",
                    CreatedDate = (DateTime)p.CreatedDate!
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return payment != null
                ? new Result(true, "Payment found", payment)
                : new Result(false, "Payment not found");
        }

        public async Task<Result> GetAllPaymentsAsync(string userId, string role)
        {
            var query = _context.Payment
                .Where(p => !p.IsDeleted);

            // Admin sees all — Lawyer and Client see only their own
            if (role != "Admin")
                query = query.Where(p => p.CreatedBy == userId);

            var payments = await query
                .Select(p => new PaymentResponseDto
                {
                    PaymentId = p.PaymentId,
                    CaseId = p.CaseId,
                    Amount = p.Amount,
                    Status = p.Status!,
                    TransactionId = p.TransactionId,
                    ValidationId = p.ValidationId,
                    PaymentMethodName = p.Method!.PaymentMethodName,
                    CreatedBy = _context.User
                        .Where(u => u.UserId == p.CreatedBy)
                        .Select(u => u.UserName)
                        .FirstOrDefault() ?? "Unknown",
                    CreatedDate = (DateTime)p.CreatedDate!
                })
                .AsNoTracking()
                .ToListAsync();

            return payments.Count > 0
                ? new Result(true, "Payments retrieved", payments)
                : new Result(false, "No payments found");
        }

        public async Task<Result> CashPaymentAsync(CashPaymentDto dto)
        {
            var userId = _accessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier);

            var payment = new Payment
            {
                Amount = dto.Amount,
                PaymentMethodId = dto.PaymentMethodId,
                CaseId = dto.CaseId,
                HearingId = dto.HearingId,
                Status = "SUCCESS",      // ✅ immediately success
                CreatedBy = userId
            };
            payment.TransactionId = payment.PaymentId;

            _context.Payment.Add(payment);

            // ✅ Mark case/hearing paid immediately
            if (dto.HearingId == null)
            {
                var caseEntity = await _context.Case
                    .FirstOrDefaultAsync(c => c.CaseId == dto.CaseId);
                if (caseEntity != null)
                {
                    caseEntity.IsConsultationFeePaid = true;
                    _context.Case.Update(caseEntity);
                }
            }
            else
            {
                var hearing = await _context.Hearing
                    .FirstOrDefaultAsync(h => h.HearingID == dto.HearingId);
                if (hearing != null)
                {
                    hearing.IsPaid = true;
                    hearing.UpdatedDate = DateTime.UtcNow;
                    _context.Hearing.Update(hearing);
                }
            }

            return await Result.DBCommitAsync(
                _context, "Cash payment recorded successfully", _logger);
        }
    }
}