namespace Business.DTO.NOC
{
    public class ApplyNOCDto
    {
        public int CaseId { get; set; }
        public string AppliedByUserId { get; set; } = string.Empty;
    }

    public class ApproveNOCDto
    {
        public int NOCId { get; set; }
        public string ApprovedByUserId { get; set; } = string.Empty;
    }

    public class RejectNOCDto
    {
        public int NOCId { get; set; }
        public string RejectedByUserId { get; set; } = string.Empty;
    }

    public class NOCResponseDto
    {
        public int NOCId { get; set; }
        public int CaseId { get; set; }
        public string CaseName { get; set; } = string.Empty;
        public string AppliedByUserId { get; set; } = string.Empty;
        public string AppliedByUserName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedByName { get; set; }
        public string? LawyerName { get; set; }
        public string? LawyerEmail { get; set; }
        public string? ClientEmail { get; set; }
    }
}