using Business.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Business.Jobs
{
    public class HearingReminderJob(
        IServiceScopeFactory scopeFactory,
        ILogger<HearingReminderJob> logger) : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<HearingReminderJob> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Hearing Reminder Job started");

            while (!stoppingToken.IsCancellationRequested)
            {
#if DEBUG
                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
#else
        var now     = DateTime.UtcNow;
        var nextRun = now.Date.AddDays(1);
        var delay   = nextRun - now;

        // ✅ CA2254 fixed — structured logging parameter
        _logger.LogInformation("Next reminder check at: {NextRun:yyyy-MM-dd HH:mm} UTC", nextRun);

        await Task.Delay(delay, stoppingToken);
#endif

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var hearingService = scope.ServiceProvider.GetRequiredService<HearingService>();
                    var result = await hearingService.SendPendingCommentRemindersAsync();

                    // ✅ CA1873 fixed — structured logging parameter
                    _logger.LogInformation("Reminder job result: {Message}", result.Message);
                }
                catch (Exception ex)
                {
                    // ✅ Exception logging already structured correctly
                    _logger.LogError(ex, "Error running hearing reminder job");
                }
            }
        }
    }
}