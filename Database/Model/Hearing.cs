using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Model
{
    public class Hearing : BaseModel
    {
        [Key]
        public int HearingID { get; set; }

        [StringLength(120)]
        public int CaseId { get; set; }

        public DateTime HearingDate { get; set; }
        public bool IsGoing { get; set; } = true;
        public bool IsPaid = false;

        [ForeignKey(nameof(CaseId))]
        public Case? Case { get; set; }
    }
}