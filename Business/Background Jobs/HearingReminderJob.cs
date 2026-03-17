using Database.Context;
using Business.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Business.DTO.Hearing;

namespace Business.Jobs
{
    public class HearingReminderJob(
        IServiceScopeFactory scopeFactory,
        ILogger<HearingReminderJob> logger) : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<HearingReminderJob> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _logger.LogInformation("HearingReminderJob started.");

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await ProcessRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing reminders");
                }

                // ✅ 2 min for testing — change to 24h for production
                await Task.Delay(TimeSpan.FromMinutes(2), ct);
            }
        }

        private async Task ProcessRemindersAsync()
        {
            // ✅ Create scope to resolve scoped services
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider
                                        .GetRequiredService<LMSContext>();
            var hearingService = scope.ServiceProvider
                                        .GetRequiredService<HearingService>();

            _logger.LogInformation(
                "Checking hearing reminders at {Time}", DateTime.UtcNow);

            var now = DateTime.UtcNow;

            // ✅ Find hearings where:
            // 1. Hearing date has passed
            // 2. Reminder NOT yet sent
            var hearingsToRemind = await context.Hearing
                .Where(h => !h.IsDeleted
                         && !h.ReminderSent
                         && h.HearingDate < now)
                .Include(h => h.Case)
                    .ThenInclude(c => c!.CaseHandlingByUser)
                .ToListAsync();

            _logger.LogInformation(
                "Found {Count} hearings to check",
                hearingsToRemind.Count);

            int sent = 0;
            int failed = 0;

            foreach (var hearing in hearingsToRemind)
            {
                if (hearing.Case?.CaseHandlingByUser == null)
                    continue;

                var lawyer = hearing.Case.CaseHandlingByUser;

                // ✅ Check if comment exists for this case
                var hasComment = await context.Comment
                    .AnyAsync(c => c.CaseId == hearing.CaseId
                                && !c.IsDeleted);

                if (hasComment)
                {
                    // ✅ Comment exists — mark done, no email
                    _logger.LogInformation(
                        "Case {CaseId} has comment — marking done",
                        hearing.CaseId);

                    hearing.ReminderSent = true;
                    context.Hearing.Update(hearing);
                    await context.SaveChangesAsync();
                    continue;
                }

                // ✅ No comment — send reminder using existing method
                _logger.LogInformation(
                    "Sending reminder for Hearing {HearingId} " +
                    "Case {CaseId} to {Email}",
                    hearing.HearingID, hearing.CaseId, lawyer.Email);

                // ✅ Build DTO to match your existing method
                var reminderDto = new PendingCommentReminderDto
                {
                    CaseId = hearing.CaseId,
                    CaseName = hearing.Case.CaseName,
                    HearingDate = hearing.HearingDate,
                    LawyerName = lawyer.UserName,
                    LawyerEmail = lawyer.Email
                };

                var result = await hearingService
                    .SendPendingCommentRemindersAsync();

                if (result.Success)
                {
                    // ✅ Mark as sent — won't send again
                    hearing.ReminderSent = true;
                    context.Hearing.Update(hearing);
                    await context.SaveChangesAsync();
                    sent++;

                    _logger.LogInformation(
                        "Reminder sent — Hearing {HearingId} " +
                        "marked ReminderSent=true",
                        hearing.HearingID);
                }
                else
                {
                    failed++;
                    _logger.LogWarning(
                        "Reminder failed for Hearing {HearingId}: {Msg}",
                        hearing.HearingID, result.Message);
                }
            }

            _logger.LogInformation(
                "Reminder job result: Sent={Sent}, Failed={Failed}",
                sent, failed);
        }
    }
}