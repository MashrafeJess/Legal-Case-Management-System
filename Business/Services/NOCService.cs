using System.Security.Claims;
using Business.DTO.NOC;
using Database.Context;
using Database.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;
using MailKit.Net.Smtp;

namespace Business.Services
{
    public class NOCService(
        LMSContext context,
        IHttpContextAccessor accessor,
        ILogger<NOCService> logger)
    {
        private readonly LMSContext _context = context;
        private readonly IHttpContextAccessor _accessor = accessor;
        private readonly ILogger<NOCService> _logger = logger;

        // ─── Apply for NOC ──────────────────────────────────────────────
        public async Task<Result> ApplyAsync(ApplyNOCDto dto)
        {
            var userId = _accessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
                return new Result(false, "Unauthorized");

            // ✅ Check case exists
            var caseEntity = await _context.Case
                .Include(c => c.CaseHandlingByUser)
                .FirstOrDefaultAsync(c => c.CaseId == dto.CaseId
                                       && !c.IsDeleted);
            if (caseEntity == null)
                return new Result(false, "Case not found");

            // ✅ Check user has at least one paid hearing
            var hasPaidHearing = await _context.Hearing
                .AnyAsync(h => h.CaseId == dto.CaseId
                            && h.IsPaid
                            && !h.IsDeleted);

            if (!hasPaidHearing)
            {
                return new Result(false,
                    "You must have at least one paid hearing " +
                    "to apply for NOC");
            }

            // ✅ Check no pending/approved NOC already exists
            var existing = await _context.NOC
                .FirstOrDefaultAsync(n => n.CaseId == dto.CaseId
                                       && !n.IsDeleted
                                       && (n.Status == "Pending"
                                        || n.Status == "Approved"));
            if (existing != null)
            {
                return new Result(false,
                    existing.Status == "Approved"
                        ? "NOC already approved for this case"
                        : "NOC application already pending");
            }

            var noc = new NOC
            {
                CaseId = dto.CaseId,
                AppliedByUserId = userId,
                Status = "Pending",
                AppliedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.NOC.Add(noc);
            return await Result.DBCommitAsync(
                _context, "NOC application submitted successfully",
                _logger, "Failed to apply for NOC", noc);
        }

        // ─── Approve NOC ────────────────────────────────────────────────
        public async Task<Result> ApproveAsync(ApproveNOCDto dto)
        {
            var approverId = _accessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier);

            var noc = await _context.NOC
                .Include(n => n.Case)
                    .ThenInclude(c => c!.CaseHandlingByUser)
                .Include(n => n.AppliedByUser)
                .FirstOrDefaultAsync(n => n.NOCId == dto.NOCId
                                       && !n.IsDeleted);
            if (noc == null)
                return new Result(false, "NOC not found");

            if (noc.Status != "Pending")
            {
                return new Result(false,
                    $"NOC is already {noc.Status}");
            }

            // ✅ Approve
            noc.Status = "Approved";
            noc.ApprovedAt = DateTime.UtcNow;
            noc.ApprovedByUserId = approverId;
            noc.UpdatedDate = DateTime.UtcNow;
            noc.UpdatedBy = approverId;

            // ✅ Close the case
            if (noc.Case != null)
            {
                noc.Case.CaseStatus = "Closed";
                noc.Case.UpdatedDate = DateTime.UtcNow;
                noc.Case.UpdatedBy = approverId;
                _context.Case.Update(noc.Case);
            }

            _context.NOC.Update(noc);
            var result = await Result.DBCommitAsync(
                _context, "NOC approved successfully", _logger);

            if (!result.Success) return result;

            // ✅ Send NOC email
            _ = Task.Run(async () =>
            {
                try
                {
                    await SendNOCEmailAsync(noc);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to send NOC email for NOCId: {NOCId}",
                        noc.NOCId);
                }
            });

            return new Result(true, "NOC approved and email sent");
        }

        // ─── Reject NOC ─────────────────────────────────────────────────
        public async Task<Result> RejectAsync(RejectNOCDto dto)
        {
            var rejecterId = _accessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier);

            var noc = await _context.NOC
                .FirstOrDefaultAsync(n => n.NOCId == dto.NOCId
                                       && !n.IsDeleted);
            if (noc == null)
                return new Result(false, "NOC not found");

            if (noc.Status != "Pending")
            {
                return new Result(false,
                    $"NOC is already {noc.Status}");
            }

            noc.Status = "Rejected";
            noc.UpdatedDate = DateTime.UtcNow;
            noc.UpdatedBy = rejecterId;

            _context.NOC.Update(noc);
            return await Result.DBCommitAsync(
                _context, "NOC rejected", _logger);
        }

        // ─── Get All NOCs ───────────────────────────────────────────────
        public async Task<Result> GetAllAsync(string userId, string role)
        {
            IQueryable<NOC> query = _context.NOC
                .Where(n => !n.IsDeleted);

            if (role == "Client")
            {
                query = query.Where(n => n.AppliedByUserId == userId);
            }
            else if (role == "Lawyer")
            {
                var lawyerCaseIds = await _context.Case
                    .Where(c => c.CaseHandlingBy == userId && !c.IsDeleted)
                    .Select(c => c.CaseId)
                    .ToListAsync();
                query = query.Where(n => lawyerCaseIds.Contains(n.CaseId));
            }

            var list = await query
                .Include(n => n.Case)
                    .ThenInclude(c => c!.CaseHandlingByUser)
                .Include(n => n.AppliedByUser)
                .Include(n => n.ApprovedByUser)
                .OrderByDescending(n => n.AppliedAt)
                .Select(n => new NOCResponseDto
                {
                    NOCId = n.NOCId,
                    CaseId = n.CaseId,
                    CaseName = n.Case!.CaseName,
                    AppliedByUserId = n.AppliedByUserId,
                    AppliedByUserName = n.AppliedByUser!.UserName,
                    Status = n.Status,
                    AppliedAt = n.AppliedAt,
                    ApprovedAt = n.ApprovedAt,
                    ApprovedByName = n.ApprovedByUser != null
                        ? n.ApprovedByUser.UserName : null,
                    LawyerName = n.Case.CaseHandlingByUser != null
                        ? n.Case.CaseHandlingByUser.UserName : null,
                    LawyerEmail = n.Case.CaseHandlingByUser != null
                        ? n.Case.CaseHandlingByUser.Email : null,
                    ClientEmail = n.AppliedByUser.Email
                })
                .AsNoTracking()
                .ToListAsync();

            return list.Count > 0
                ? new Result(true, "NOCs retrieved", list)
                : new Result(false, "No NOCs found");
        }

        // ─── Get NOC by CaseId ──────────────────────────────────────────
        public async Task<Result> GetByCaseIdAsync(int caseId)
        {
            var noc = await _context.NOC
                .Where(n => n.CaseId == caseId && !n.IsDeleted)
                .Include(n => n.AppliedByUser)
                .Include(n => n.ApprovedByUser)
                .OrderByDescending(n => n.AppliedAt)
                .Select(n => new NOCResponseDto
                {
                    NOCId = n.NOCId,
                    CaseId = n.CaseId,
                    CaseName = n.Case!.CaseName,
                    AppliedByUserId = n.AppliedByUserId,
                    AppliedByUserName = n.AppliedByUser!.UserName,
                    Status = n.Status,
                    AppliedAt = n.AppliedAt,
                    ApprovedAt = n.ApprovedAt,
                    ApprovedByName = n.ApprovedByUser != null
                        ? n.ApprovedByUser.UserName : null,
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return noc != null
                ? new Result(true, "NOC found", noc)
                : new Result(false, "No NOC found for this case");
        }

        // ─── Send NOC Email ─────────────────────────────────────────────
        private async Task SendNOCEmailAsync(NOC noc)
        {
            var smtp = await _context.SmtpSettings
                .FirstOrDefaultAsync(s => !s.IsDeleted);
            if (smtp == null)
            {
                _logger.LogWarning("No SMTP config found");
                return;
            }

            var client = noc.AppliedByUser;
            var lawyer = noc.Case?.CaseHandlingByUser;

            if (client == null)
            {
                _logger.LogWarning("Client not found for NOC email");
                return;
            }

            var issuedDate = DateTime.UtcNow.ToString("MMMM dd, yyyy");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                "Legal Case Management", smtp.SenderEmail));
            message.To.Add(new MailboxAddress(
                client.UserName, client.Email));
            message.Subject =
                $"✅ No Objection Certificate — {noc.Case?.CaseName}";

            message.Body = new BodyBuilder
            {
                HtmlBody = $"""

                <!DOCTYPE html>
                <html>
                <head><meta charset="utf-8"></head>
                <body style="font-family:Arial,sans-serif;
                              background:#f5f5f5; padding:20px;">
                    <div style="max-width:620px; margin:0 auto;
                                 background:white; border-radius:8px;
                                 overflow:hidden;
                                 box-shadow:0 2px 8px
                                   rgba(0,0,0,0.1);">

                        <!-- Header -->
                        <div style="background:linear-gradient(
                                       135deg,#1a56db,#1e40af);
                                     color:white; padding:30px 20px;
                                     text-align:center;">
                            <h1 style="margin:0; font-size:26px;">
                                ⚖️ No Objection Certificate
                            </h1>
                            <p style="margin:5px 0 0; opacity:0.9;">
                                Legal Case Management System
                            </p>
                        </div>

                        <!-- Certificate Body -->
                        <div style="padding:40px 30px;">

                            <div style="text-align:center;
                                         margin-bottom:30px;">
                                <span style="background:#10b981;
                                              color:white;
                                              padding:8px 20px;
                                              border-radius:20px;
                                              font-weight:bold;
                                              font-size:14px;">
                                    ✅ APPROVED
                                </span>
                            </div>

                            <p style="font-size:15px;">
                                Dear <strong>{client.UserName}</strong>,
                            </p>

                            <p style="font-size:15px; line-height:1.6;">
                                This is to certify that the legal
                                proceedings associated with the case
                                mentioned below have been concluded
                                satisfactorily. A
                                <strong>No Objection Certificate</strong>
                                is hereby issued upon your request.
                            </p>

                            <!-- Certificate Details -->
                            <div style="background:#f8faff;
                                         border:2px solid #e0e7ff;
                                         border-radius:8px;
                                         padding:20px;
                                         margin:25px 0;">
                                <h3 style="margin:0 0 15px;
                                             color:#1a56db;
                                             font-size:16px;">
                                    Certificate Details
                                </h3>
                                <table style="width:100%;
                                              border-collapse:collapse;">
                                    <tr>
                                        <td style="padding:8px 0;
                                                    color:#6b7280;
                                                    font-weight:600;
                                                    width:40%;">
                                            Certificate No.
                                        </td>
                                        <td style="padding:8px 0;
                                                    font-weight:bold;
                                                    color:#1a56db;">
                                            NOC-{noc.NOCId:D6}
                                        </td>
                                    </tr>
                                    <tr style="border-top:
                                                1px solid #e5e7eb;">
                                        <td style="padding:8px 0;
                                                    color:#6b7280;
                                                    font-weight:600;">
                                            Case Name
                                        </td>
                                        <td style="padding:8px 0;">
                                            {noc.Case?.CaseName}
                                        </td>
                                    </tr>
                                    <tr style="border-top:
                                                1px solid #e5e7eb;">
                                        <td style="padding:8px 0;
                                                    color:#6b7280;
                                                    font-weight:600;">
                                            Case ID
                                        </td>
                                        <td style="padding:8px 0;">
                                            #{noc.CaseId}
                                        </td>
                                    </tr>
                                    <tr style="border-top:
                                                1px solid #e5e7eb;">
                                        <td style="padding:8px 0;
                                                    color:#6b7280;
                                                    font-weight:600;">
                                            Client Name
                                        </td>
                                        <td style="padding:8px 0;">
                                            {client.UserName}
                                        </td>
                                    </tr>
                                    <tr style="border-top:
                                                1px solid #e5e7eb;">
                                        <td style="padding:8px 0;
                                                    color:#6b7280;
                                                    font-weight:600;">
                                            Lawyer
                                        </td>
                                        <td style="padding:8px 0;">
                                            {lawyer?.UserName ?? "—"}
                                        </td>
                                    </tr>
                                    <tr style="border-top:
                                                1px solid #e5e7eb;">
                                        <td style="padding:8px 0;
                                                    color:#6b7280;
                                                    font-weight:600;">
                                            Date Issued
                                        </td>
                                        <td style="padding:8px 0;">
                                            {issuedDate}
                                        </td>
                                    </tr>
                                    <tr style="border-top:
                                                1px solid #e5e7eb;">
                                        <td style="padding:8px 0;
                                                    color:#6b7280;
                                                    font-weight:600;">
                                            Case Status
                                        </td>
                                        <td style="padding:8px 0;">
                                            <span style="background:
                                                          #e5e7eb;
                                                          padding:
                                                          3px 10px;
                                                          border-radius:
                                                          20px;
                                                          font-size:
                                                          13px;">
                                                🔒 Closed
                                            </span>
                                        </td>
                                    </tr>
                                </table>
                            </div>

                            <p style="font-size:14px;
                                       color:#6b7280;
                                       line-height:1.6;">
                                This certificate confirms that there
                                are no pending legal objections or
                                disputes related to the above case.
                                The case has been officially
                                <strong>closed</strong>.
                            </p>

                            <div style="background:#fef3c7;
                                         border-left:4px solid #f59e0b;
                                         padding:15px;
                                         border-radius:4px;
                                         margin-top:20px;">
                                <p style="margin:0; font-size:13px;
                                           color:#92400e;">
                                    <strong>📌 Important:</strong>
                                    Please save this email as your
                                    official NOC document. This is
                                    a system-generated certificate.
                                </p>
                            </div>
                        </div>

                        <!-- Footer -->
                        <div style="background:#f9fafb;
                                     padding:20px;
                                     text-align:center;
                                     color:#6b7280;
                                     font-size:13px;
                                     border-top:1px solid #e5e7eb;">
                            <p style="margin:0;">
                                This is an automated certificate
                                from Legal Case Management System.
                            </p>
                            <p style="margin:5px 0 0;">
                                © {DateTime.UtcNow.Year}
                                Legal Case Management System.
                                All rights reserved.
                            </p>
                        </div>
                    </div>
                </body>
                </html>
"""
            }.ToMessageBody();

            try
            {
                using var smtpClient = new SmtpClient();
                await smtpClient.ConnectAsync(
                    smtp.Host, smtp.Port,
                    smtp.EnableSsl
                        ? MailKit.Security.SecureSocketOptions.StartTls
                        : MailKit.Security.SecureSocketOptions.None);
                await smtpClient.AuthenticateAsync(
                    smtp.Username, smtp.Password);
                await smtpClient.SendAsync(message);
                await smtpClient.DisconnectAsync(true);

                _logger.LogInformation(
                    "NOC email sent to {Email}", client.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send NOC email to {Email}", client.Email);
                throw;
            }
        }
    }
}