using System.Security.Claims;
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
                Status = "PENDING",
                CreatedBy = userId
            };

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
            var payment = await _context.Payment
                .FirstOrDefaultAsync(p => p.PaymentId == tranId && !p.IsDeleted);

            if (payment == null)
                return new Result(false, "Payment record not found");

            // Don't process already completed payments
            if (payment.Status == "SUCCESS")
                return new Result(true, "Payment already processed");

            // Validate with SSLCommerz
            var environment = _settings.IsSandbox
                ? SSLCommerzPaymentEnvironment.SANDBOX
                : SSLCommerzPaymentEnvironment.LIVE;

            var validation = await AITSSLCommerzClient.GetPaymentValidationStatusAsync(
                validationId: valId,
                storeId: _settings.StoreId,
                storePassword: _settings.StorePassword,
                environment: environment
            );

            if (!validation.IsSuccess)
                return new Result(false, "Payment validation failed");

            payment.Status = "SUCCESS";
            payment.ValidationId = valId;
            payment.TransactionId = tranId;
            payment.UpdatedBy = "SYSTEM";
            payment.UpdatedDate = DateTime.UtcNow;

            _context.Payment.Update(payment);
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
    }
}