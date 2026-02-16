using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Model
{
    public class Case : BaseModel
    {
        [Key]
        [Required]
        public int CaseId { get; set; }

        [Required]
        public string Email { get; set; } = string.Empty;

        [StringLength(50)]
        public string CaseName { get; set; } = String.Empty;

        public int? CaseTypeId { get; set; }

        [StringLength(120)]
        public string CaseHandlingBy { get; set; } = string.Empty;

        public int HearingNumber { get; set; }
        public int Fee { get; set; }
        public bool IsConsultationFeePaid { get; set; } = false;

        [ForeignKey(nameof(CaseHandlingBy))]
        public User? CaseHandlingByUser { get; set; }

        [ForeignKey(nameof(CaseTypeId))]
        public CaseType? Type { get; set; }

        public ICollection<FileEntity>? Files { get; set; }
    }
}