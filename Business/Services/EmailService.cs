using System.Security.Claims;
using Business.DTO.Hearing;
using Business.DTO.Mail;
using Database.Context;
using Database.Model;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace Business.Services
{
    public class EmailService(
        SmtpService smtpService,
        OTPService otpService,
        LMSContext context,
        IHttpContextAccessor accessor)
    {
        private readonly SmtpService _smtpService = smtpService;
        private readonly OTPService _otpService = otpService;
        private readonly LMSContext _context = context;
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
    }
}