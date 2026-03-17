using System.Drawing;
using System.Security.Claims;
using Business.DTO.Hearing;
using Business.DTO.Mail;
using Database.Context;
using Database.Model;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;
using Npgsql.Internal;
using static System.Net.Mime.MediaTypeNames;

namespace Business.Services
{
    public class EmailService(
        SmtpService smtpService,
        OTPService otpService,
        LMSContext context,
        ILogger<EmailService> logger,
        IHttpContextAccessor accessor)
    {
        private readonly SmtpService _smtpService = smtpService;
        private readonly OTPService _otpService = otpService;
        private readonly LMSContext _context = context;
        private readonly ILogger<EmailService> _logger = logger;
        private readonly IHttpContextAccessor _accessor = accessor;

        // ─── Existing OTP Email ─────────────────────────────────────────
        public async Task<Result> SendOtpEmailAsync(string toEmail)
        {
            var smtpConfigResult = await _smtpService.GetSmtp(1);
            if (!(smtpConfigResult.Success && smtpConfigResult.Data is SmtpSettings smtpConfig))
                return new Result(false, "SMTP Configuration Not Found");

            var otpResult = await _otpService.CreateToken(toEmail);
            if (otpResult.Data is not Token token)
                return new Result(false, "Failed to generate OTP token");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(smtpConfig.SenderEmail, smtpConfig.SenderEmail));
            message.To.Add(new MailboxAddress(toEmail, toEmail));
            message.Subject = "Otp Mail";
            message.Body = new BodyBuilder
            {
                HtmlBody = $"""
                    <div style="font-family: Arial, sans-serif; padding: 20px;">
                        <h3>Your OTP Code</h3>
                        <p style="font-size: 24px; font-weight: bold;">{token.TokenId}</p>
                        <small>This OTP is valid for a limited time. Do not share it.</small>
                    </div>
                    """
            }.ToMessageBody();

            return await SendAsync(smtpConfig, message);
        }

        // ─── Dynamic User to User Email ─────────────────────────────────
        public async Task<Result> SendMailToUserAsync(SendMailDto dto)
        {
            // Get logged-in sender from JWT
            var senderId = _accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(senderId))
                return new Result(false, "Unauthorized — please login first");

            // Prevent sending to yourself
            if (senderId == dto.ReceiverUserId)
                return new Result(false, "You cannot send an email to yourself");

            // Fetch both sender and receiver in a single query
            var users = await _context.User
                .Where(u => (u.UserId == senderId || u.UserId == dto.ReceiverUserId) && !u.IsDeleted)
                .Select(u => new { u.UserId, u.UserName, u.Email })
                .AsNoTracking()
                .ToListAsync();

            var sender = users.FirstOrDefault(u => u.UserId == senderId);
            var receiver = users.FirstOrDefault(u => u.UserId == dto.ReceiverUserId);

            if (sender == null)
                return new Result(false, "Sender account not found");

            if (receiver == null)
                return new Result(false, "Receiver not found");

            // Get SMTP config
            var smtpConfigResult = await _smtpService.GetSmtp(1);
            if (!(smtpConfigResult.Success && smtpConfigResult.Data is SmtpSettings smtpConfig))
                return new Result(false, "SMTP Configuration Not Found");

            // Build message
            var message = new MimeMessage();

            // System email sends on behalf of sender
            message.From.Add(new MailboxAddress($"{sender.UserName} via LCMS", smtpConfig.SenderEmail));

            // Reply-To ensures replies go to actual sender
            message.ReplyTo.Add(new MailboxAddress(sender.UserName, sender.Email));

            message.To.Add(new MailboxAddress(receiver.UserName, receiver.Email));
            message.Subject = dto.Subject;

            message.Body = new BodyBuilder
            {
                HtmlBody = $"""
                    <div style="font-family: Arial, sans-serif; padding: 20px; max-width: 600px;">
                        <div style="background:#f5f5f5; padding:10px 20px; border-radius:5px; margin-bottom:20px;">
                            <p><b>From:</b> {sender.UserName} ({sender.Email})</p>
                            <p><b>To:</b>   {receiver.UserName} ({receiver.Email})</p>
                        </div>
                        <div style="padding: 10px 0;">
                            {dto.Body}
                        </div>
                        <hr/>
                        <small style="color:#888;">
                            Sent via Legal Case Management System.
                            Reply to this email to respond directly to {sender.UserName}.
                        </small>
                    </div>
                    """
            }.ToMessageBody();

            var result = await SendAsync(smtpConfig, message);

            if (!result.Success)
                return result;

            _logger.LogInformation("Mail sent — now saving log to DB");

            var log = new MailLog
            {
                SenderUserId = senderId!,
                ReceiverUserId = dto.ReceiverUserId,
                Subject = dto.Subject,
                Body = dto.Body,
                SentAt = DateTime.UtcNow,
                CreatedBy = senderId
            };

            _context.MailLog.Add(log);

            _logger.LogInformation("MailLog created — SenderUserId: {S}, ReceiverUserId: {R}",
                log.SenderUserId, log.ReceiverUserId);

            var result1 = await Result.DBCommitAsync(
                _context, "Mail sent successfully", _logger);
            if (!result1.Success)
            {
                return result1;
            }
            // Return mail summary on success
            return new Result(true, "Email sent successfully", new MailResponseDto
            {
                SenderName = sender.UserName,
                SenderEmail = sender.Email,
                ReceiverName = receiver.UserName,
                ReceiverEmail = receiver.Email,
                Subject = dto.Subject,
                SentAt = DateTime.UtcNow
            });
        }

        // ─── Hearing Comment Reminder ────────────────────────────────────
        public async Task<Result> SendReminderEmailAsync(PendingCommentReminderDto hearing)
        {
            var smtpConfigResult = await _smtpService.GetSmtp(1);
            if (!(smtpConfigResult.Success && smtpConfigResult.Data is SmtpSettings smtpConfig))
                return new Result(false, "SMTP Configuration Not Found");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("LCMS Reminder", smtpConfig.SenderEmail));
            message.To.Add(new MailboxAddress(hearing.LawyerName, hearing.LawyerEmail));
            message.Subject = $"⚠️ Reminder: Case Comment Required — {hearing.CaseName}";

            message.Body = new BodyBuilder
            {
                HtmlBody = $"""
            <div style="font-family: Arial, sans-serif; padding: 20px; max-width: 600px;">
                <div style="background: #fff3cd; border-left: 5px solid #ffc107; padding: 15px; border-radius: 4px;">
                    <h3 style="margin:0; color:#856404;">⚠️ Case Comment Pending</h3>
                </div>
                <br/>
                <p>Dear <b>{hearing.LawyerName}</b>,</p>
                <p>
                    This is a reminder that you have not added a case comment
                    following the hearing scheduled on
                    <b>{hearing.HearingDate:dddd, MMMM dd yyyy}</b>
                    for case <b>{hearing.CaseName}</b>.
                </p>
                <p>Please log in to the system and add your update at your earliest convenience.</p>
                <br/>
                <table style="border-collapse: collapse; width: 100%;">
                    <tr style="background: #f8f9fa;">
                        <td style="padding: 8px; border: 1px solid #dee2e6;"><b>Case ID</b></td>
                        <td style="padding: 8px; border: 1px solid #dee2e6;">{hearing.CaseId}</td>
                    </tr>
                    <tr>
                        <td style="padding: 8px; border: 1px solid #dee2e6;"><b>Case Name</b></td>
                        <td style="padding: 8px; border: 1px solid #dee2e6;">{hearing.CaseName}</td>
                    </tr>
                    <tr style="background: #f8f9fa;">
                        <td style="padding: 8px; border: 1px solid #dee2e6;"><b>Hearing Date</b></td>
                        <td style="padding: 8px; border: 1px solid #dee2e6;">{hearing.HearingDate:yyyy-MM-dd HH:mm} UTC</td>
                    </tr>
                </table>
                <br/>
                <hr/>
                <small style="color: #888;">
                    This is an automated reminder from the Legal Case Management System.
                    Please do not reply to this email.
                </small>
            </div>
            """
            }.ToMessageBody();

            return await SendAsync(smtpConfig, message);
        }

        //--- Confirmation Mail After Payment -----
        public async Task<Result> SendPaymentConfirmationEmailAsync(Payment payment)
        {
            // Get user who paid
            var user = await _context.User
                .FirstOrDefaultAsync(u => u.UserId == payment.CreatedBy);
            if (user == null)
            {
                _logger.LogWarning("User not found for payment confirmation: {UserId}", payment.CreatedBy);
                return new Result(false, "");
            }

            // Get case details
            var caseData = await _context.Case
                .Include(c => c.Type)
                .Include(c => c.CaseHandlingByUser)
                .FirstOrDefaultAsync(c => c.CaseId == payment.CaseId);
            if (caseData == null)
            {
                _logger.LogWarning("Case not found for payment confirmation: {CaseId}", payment.CaseId);
                return new Result(false, "Case Data not found");
            }

            // Get payment method
            var method = await _context.PaymentMethod
                .FirstOrDefaultAsync(m => m.PaymentMethodId == payment.PaymentMethodId);

            // Get hearing if applicable
            string paymentType = "Consultation Fee";
            string hearingDate = "—";
            if (payment.HearingId != null)
            {
                var hearing = await _context.Hearing
                    .FirstOrDefaultAsync(h => h.HearingID == payment.HearingId);
                if (hearing != null)
                {
                    paymentType = "Hearing Fee";
                    hearingDate = hearing.HearingDate.ToString("MMMM dd, yyyy");
                }
            }

            // Get SMTP config
            var smtp = await _context.SmtpSettings.FirstOrDefaultAsync(s => !s.IsDeleted);
            if (smtp == null)
            {
                _logger.LogWarning("No SMTP configuration found");
                return new Result(false, "SMTP configuration not found");
            }

            // ✅ Build receipt email
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Legal Case Management", smtp.SenderEmail));
            message.To.Add(new MailboxAddress(user.UserName, user.Email));
            message.Subject = $"Payment Receipt — {paymentType} Confirmed";

            var htmlBody = $$"""
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <style>
        body { font-family: Arial, sans-serif; background: #f5f5f5; margin: 0; padding: 20px; }
        .container { max-width: 600px; margin: 0 auto; background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }
        .header { background: linear-gradient(135deg, #1a56db 0%, #1e40af 100%); color: white; padding: 30px 20px; text-align: center; }
        .header h1 { margin: 0; font-size: 28px; }
        .header p { margin: 5px 0 0; opacity: 0.9; }
        .content { padding: 30px 20px; }
        .success-badge { background: #10b981; color: white; display: inline-block; padding: 8px 16px; border-radius: 20px; font-weight: bold; margin-bottom: 20px; }
        .info-table { width: 100%; border-collapse: collapse; margin: 20px 0; }
        .info-table td { padding: 12px; border-bottom: 1px solid #e5e7eb; }
        .info-table td:first-child { font-weight: 600; color: #6b7280; width: 40%; }
        .info-table td:last-child { color: #1f2937; }
        .amount-box { background: #eff6ff; border: 2px solid #3b82f6; border-radius: 8px; padding: 20px; text-align: center; margin: 20px 0; }
        .amount-box .label { color: #6b7280; font-size: 14px; margin-bottom: 5px; }
        .amount-box .amount { font-size: 36px; font-weight: bold; color: #1a56db; }
        .footer { background: #f9fafb; padding: 20px; text-align: center; color: #6b7280; font-size: 14px; border-top: 1px solid #e5e7eb; }
        .footer a { color: #1a56db; text-decoration: none; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>⚖️ Payment Receipt</h1>
            <p>Legal Case Management System</p>
        </div>
        <div class="content">
            <div class="success-badge">✅ Payment Confirmed</div>
            <p>Dear <strong>{{user.UserName}}</strong>,</p>
            <p>Your payment has been successfully processed. Here are the details:</p>
            
            <div class="amount-box">
                <div class="label">Amount Paid</div>
                <div class="amount">৳ {{payment.Amount:N0}}</div>
            </div>

            <table class="info-table">
                <tr>
                    <td>Transaction ID</td>
                    <td><strong>{{payment.TransactionId}}</strong></td>
                </tr>
                <tr>
                    <td>Payment Type</td>
                    <td>{{paymentType}}</td>
                </tr>
                <tr>
                    <td>Payment Method</td>
                    <td>{{method?.PaymentMethodName ?? "Online"}}</td>
                </tr>
                <tr>
                    <td>Date & Time</td>
                    <td>{{DateTime.UtcNow:MMMM dd, yyyy 'at' hh:mm tt}}</td>
                </tr>
                <tr>
                    <td>Case Name</td>
                    <td>{{caseData.CaseName}}</td>
                </tr>
                <tr>
                    <td>Case Type</td>
                    <td>{{caseData.Type?.CaseTypeName ?? "—"}}</td>
                </tr>
                <tr>
                    <td>Lawyer</td>
                    <td>{{caseData.CaseHandlingByUser?.UserName ?? "—"}}</td>
                </tr>
                {{(payment.HearingId != null ? $@"
                <tr>
                    <td>Hearing Date</td>
                    <td>{hearingDate}</td>
                </tr>" : "")}}
            </table>

            <p style="margin-top: 30px; padding: 15px; background: #fef3c7; border-left: 4px solid #f59e0b; border-radius: 4px; color: #92400e; font-size: 14px;">
                <strong>📌 Note:</strong> Please keep this receipt for your records. 
                If you have any questions, contact your assigned lawyer.
            </p>
        </div>
        <div class="footer">
            <p>This is an automated receipt. Please do not reply to this email.</p>
            <p>© {{DateTime.UtcNow.Year}} Legal Case Management System. All rights reserved.</p>
        </div>
    </div>
</body>
</html>

""";

            message.Body = new BodyBuilder
            {
                HtmlBody = htmlBody
            }.ToMessageBody();

            // ✅ Send email
            var result = await SendAsync(smtp, message);
            if(!result.Success)
            {
                return result;
            }
            return result;
        }

        // ─── Private: Shared SMTP send logic ────────────────────────────
        private static async Task<Result> SendAsync(SmtpSettings smtpConfig, MimeMessage message)
        {
            try
            {
                using var client = new SmtpClient();

                SecureSocketOptions sslOption = smtpConfig.EnableSsl
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.None;

                await client.ConnectAsync(smtpConfig.Host, smtpConfig.Port, sslOption);
                await client.AuthenticateAsync(smtpConfig.Username, smtpConfig.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return new Result(true, "Email sent successfully");
            }
            catch (Exception ex)
            {
                return new Result(false, $"Failed to send email: {ex.Message}");
            }
        }

        public async Task<Result> GetAllAsync(string userId, string role)
        {
            var query = _context.MailLog
                .Where(m => !m.IsDeleted);

            // Admin sees all — Lawyer/Client see only their own
            if (role != "Admin")
                query = query.Where(m => m.SenderUserId == userId);

            var logs = await query
                .OrderByDescending(m => m.SentAt)
                .Select(m => new MailResponseDto
                {
                    MailLogId = m.MailLogId,
                    SenderName = m.Sender!.UserName ?? "Unknown",
                    SenderEmail = m.Sender!.Email ?? "Unknown",
                    ReceiverName = m.Receiver!.UserName ?? "Unknown",
                    ReceiverEmail = m.Receiver!.Email ?? "Unknown",
                    Subject = m.Subject,
                    Body = m.Body,
                    SentAt = m.SentAt
                })
                .AsNoTracking()
                .ToListAsync();

            return logs.Count > 0
                ? new Result(true, "Mail logs retrieved", logs)
                : new Result(false, "No mail logs found");
        }
    }
}