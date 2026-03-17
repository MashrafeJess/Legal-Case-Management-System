namespace Business.DTO.Report
{
    public class MonthlyReportDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int TotalEarning { get; set; }
        public int TotalSalaryCost { get; set; }
        public int Profit { get; set; }
        public int TotalCasesHandled { get; set; }
        public int CasesWon { get; set; }
        public int CasesLost { get; set; }
        public int CasesOngoing { get; set; }
    }

    public class ReportRequestDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
    }
}