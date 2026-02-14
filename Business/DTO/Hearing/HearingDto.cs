namespace Business.DTO.Hearing
{
    public class CreateHearingDto
    {
        public int CaseId { get; set; }
        public DateTime HearingDate { get; set; }
        public bool IsGoing { get; set; }
        public bool IsPaid { get; set; }
    }
    public class UpdateHearingDto
    {
        public int HearingId { get; set; }
        public int CaseId { get; set; }
        public DateTime HearingDate { get; set; }
        public bool IsGoing { get; set; }
        public bool IsPaid { get; set; }
    }
    public class PendingCommentReminderDto
    {
        public int HearingId { get; set; }
        public int CaseId { get; set; }
        public string CaseName { get; set; } = string.Empty;
        public DateTime HearingDate { get; set; }
        public string LawyerName { get; set; } = string.Empty;
        public string LawyerEmail { get; set; } = string.Empty;
    }
}