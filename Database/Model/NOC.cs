using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Model
{
    public class NOC : BaseModel
    {
        [Key]
        public int NOCId { get; set; }

        public int CaseId { get; set; }

        [StringLength(50)]
        public string AppliedByUserId { get; set; } = string.Empty;

        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending/Approved/Rejected

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }

        [StringLength(50)]
        public string? ApprovedByUserId { get; set; }

        // Navigation
        [ForeignKey(nameof(CaseId))]
        public Case? Case { get; set; }

        [ForeignKey(nameof(AppliedByUserId))]
        public User? AppliedByUser { get; set; }

        [ForeignKey(nameof(ApprovedByUserId))]
        public User? ApprovedByUser { get; set; }
    }
}