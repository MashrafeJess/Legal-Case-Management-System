using System.Security.Claims;
using Business.DTO.Hearing;
using Database.Context;
using Database.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Business.Services
{
    public class HearingService(
        LMSContext context,
        EmailService emailService,
        IHttpContextAccessor accessor,
        ILogger<HearingService> logger)
    {
        private readonly LMSContext _context = context;
        private readonly EmailService _emailService = emailService;
        private readonly IHttpContextAccessor _accessor = accessor;
        private readonly ILogger<HearingService> _logger = logger;

        // ─── Add Hearing ────────────────────────────────────────────────
        public async Task<Result> AddHearing(CreateHearingDto dto)
        {
            var userId = _accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            var entity = new Hearing
            {
                CaseId = dto.CaseId,
                HearingDate = dto.HearingDate,
                IsGoing = dto.IsGoing,
                CreatedBy = userId
            };

            await _context.Hearing.AddAsync(entity);
            return await Result.DBCommitAsync(_context, "Hearing added successfully", _logger, "Hearing create failed", entity);
        }

        // ─── Update Hearing ─────────────────────────────────────────────
        public async Task<Result> UpdateHearing(UpdateHearingDto dto)
        {
            var userId = _accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            var entity = await _context.Hearing
                .FirstOrDefaultAsync(u => u.HearingID == dto.HearingId && !u.IsDeleted);

            if (entity == null)
                return new Result(false, "Hearing not found");

            if (dto.HearingDate != default)
                entity.HearingDate = dto.HearingDate;

            entity.IsGoing = dto.IsGoing;
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.UtcNow;

            _context.Hearing.Update(entity);
            return await Result.DBCommitAsync(_context, "Hearing updated successfully", _logger, data: entity);
        }

        // ─── Soft Delete Hearing ────────────────────────────────────────
        public async Task<Result> DeleteHearing(int hearingId)
        {
            var userId = _accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            var entity = await _context.Hearing
                .FirstOrDefaultAsync(u => u.HearingID == hearingId && !u.IsDeleted);

            if (entity == null)
                return new Result(false, "Hearing not found");

            entity.IsDeleted = true;
            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = userId;

            _context.Hearing.Update(entity);
            return await Result.DBCommitAsync(_context, "Hearing deleted successfully", _logger);
        }

        // ─── All Hearings ───────────────────────────────────────────────
        public async Task<Result> AllHearings()
        {
            var list = await _context.Hearing
                .Where(h => !h.IsDeleted)
                .Select(h => new
                {
                    HearingId = h.HearingID,
                    h.CaseId,
                    h.HearingDate,
                    h.IsGoing,
                })
                .AsNoTracking()
                .ToListAsync();

            return list.Count > 0
                ? new Result(true, "All hearings found", list)
                : new Result(false, "No hearings found");
        }

        // ─── Hearing By Id ──────────────────────────────────────────────
        public async Task<Result> HearingById(int hearingId)
        {
            var entity = await _context.Hearing
                .Where(h => h.HearingID == hearingId && !h.IsDeleted)
                .Select(h => new
                {
                    HearingId = h.HearingID,
                    h.CaseId,
                    h.HearingDate,
                    h.IsGoing,
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return entity != null
                ? new Result(true, "Hearing found", entity)
                : new Result(false, "Hearing not found");
        }

        // ─── Check & Send Reminder Emails ───────────────────────────────
        public async Task<Result> SendPendingCommentRemindersAsync()
        {
            var now = DateTime.UtcNow;

            // Find hearings that:
            // 1. Already happened
            // 2. Not deleted
            // 3. Have no comment added AFTER the hearing date for that case
            var pendingHearings = await _context.Hearing
                .Where(h => !h.IsDeleted && h.HearingDate < now)
                .Where(h => !_context.Comment.Any(c =>
                    c.CaseId == h.CaseId &&
                    !c.IsDeleted &&
                    c.CreatedDate > h.HearingDate))  // no comment after hearing
                .Select(h => new PendingCommentReminderDto
                {
                    HearingId = h.HearingID,
                    CaseId = h.CaseId,
                    HearingDate = h.HearingDate,

                    // Get case name and lawyer info via CaseId
                    CaseName = _context.Case
                        .Where(c => c.CaseId == h.CaseId)
                        .Select(c => c.CaseName)
                        .FirstOrDefault() ?? "Unknown Case",

                    LawyerName = _context.Case
                        .Where(c => c.CaseId == h.CaseId)
                        .Select(c => c.CaseHandlingByUser!.UserName)
                        .FirstOrDefault() ?? "Unknown Lawyer",

                    LawyerEmail = _context.Case
                        .Where(c => c.CaseId == h.CaseId)
                        .Select(c => c.CaseHandlingByUser!.Email)
                        .FirstOrDefault() ?? string.Empty,
                })
                .AsNoTracking()
                .ToListAsync();

            if (pendingHearings.Count == 0)
                return new Result(true, "No pending reminders");

            int sent = 0;
            int failed = 0;

            foreach (var hearing in pendingHearings)
            {
                if (string.IsNullOrWhiteSpace(hearing.LawyerEmail))
                {
                    failed++;
                    continue;
                }

                var result = await _emailService.SendReminderEmailAsync(hearing);

                if (result.Success) sent++;
                else failed++;
            }

            return new Result(true, $"Reminders sent: {sent}, Failed: {failed}");
        }

        public async Task<Result> GetHearingsByCaseIdAsync(int caseId)
        {
            var hearings = await _context.Hearing
                .Where(h => h.CaseId == caseId && !h.IsDeleted)
                .Select(h => new
                {
                    h.HearingID,
                    h.CaseId,
                    h.HearingDate,
                    h.IsGoing,
                    h.IsPaid,
                    h.CreatedBy,
                    h.CreatedDate
                })
                .AsNoTracking()
                .ToListAsync();

            return hearings.Count > 0
                ? new Result(true, "Hearings found", hearings)
                : new Result(false, "No hearings found for this case");
        }
    }
}