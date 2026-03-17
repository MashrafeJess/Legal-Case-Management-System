using Business.DTO.Report;
using Database.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Business.Services
{
    public class ReportService(
        LMSContext context,
        ILogger<ReportService> logger)
    {
        private readonly LMSContext _context = context;
        private readonly ILogger<ReportService> _logger = logger;

        public async Task<Result> GetMonthlyReportAsync(int year, int month)
        {
            if (month < 1 || month > 12)
                return new Result(false, "Invalid month. Must be 1-12.");

            if (year < 2000 || year > 2100)
                return new Result(false, "Invalid year.");

            // ✅ Use UTC dates
            var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1).AddDays(-1)
                                     .AddHours(23).AddMinutes(59).AddSeconds(59);

            _logger.LogInformation(
                "Generating report for {Year}-{Month:D2} ({Start} to {End})",
                year, month, startDate.ToString("yyyy-MM-dd"),
                endDate.ToString("yyyy-MM-dd"));

            // ✅ Total Earning — all successful payments in this month
            var totalEarning = await _context.Payment
                .Where(p => p.Status == "SUCCESS"
                         && !p.IsDeleted
                         && p.CreatedDate >= startDate
                         && p.CreatedDate <= endDate)
                .SumAsync(p => p.Amount);

            // ✅ Total Salary Cost — all lawyers with salary
            var totalSalaryCost = await _context.User
                .Where(u => u.RoleId == 2  // Lawyer role
                         && u.SalaryId != null
                         && !u.IsDeleted)
                .Include(u => u.Salary)
                .SumAsync(u => u.Salary!.Amount);

            // ✅ Profit
            var profit = totalEarning - totalSalaryCost;

            // ✅ Cases handled this month — cases created in this month
            var casesHandled = await _context.Case
                .Where(c => !c.IsDeleted
                         && c.CreatedDate >= startDate
                         && c.CreatedDate <= endDate)
                .CountAsync();

            // ✅ Cases won/lost/ongoing — check status
            var casesWon = await _context.Case
                .Where(c => !c.IsDeleted
                         && c.CaseStatus == "Won"
                         && c.CreatedDate >= startDate
                         && c.CreatedDate <= endDate)
                .CountAsync();

            var casesLost = await _context.Case
                .Where(c => !c.IsDeleted
                         && c.CaseStatus == "Lost"
                         && c.CreatedDate >= startDate
                         && c.CreatedDate <= endDate)
                .CountAsync();

            var casesOngoing = await _context.Case
                .Where(c => !c.IsDeleted
                         && c.CaseStatus == "Ongoing"
                         && c.CreatedDate >= startDate
                         && c.CreatedDate <= endDate)
                .CountAsync();

            var report = new MonthlyReportDto
            {
                Year = year,
                Month = month,
                MonthName = startDate.ToString("MMMM yyyy"),
                TotalEarning = totalEarning,
                TotalSalaryCost = totalSalaryCost,
                Profit = profit,
                TotalCasesHandled = casesHandled,
                CasesWon = casesWon,
                CasesLost = casesLost,
                CasesOngoing = casesOngoing
            };

            return new Result(true, "Report generated successfully", report);
        }

        // ✅ Get last 12 months reports
        public async Task<Result> GetYearlyReportsAsync(int year)
        {
            var reports = new List<MonthlyReportDto>();

            for (int month = 1; month <= 12; month++)
            {
                var result = await GetMonthlyReportAsync(year, month);
                if (result.Success && result.Data != null)
                {
                    reports.Add((MonthlyReportDto)result.Data);
                }
            }

            return new Result(true, $"Yearly report for {year}", reports);
        }
    }
}