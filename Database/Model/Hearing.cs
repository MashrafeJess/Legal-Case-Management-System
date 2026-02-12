using System.ComponentModel.DataAnnotations;

namespace Database.Model
{
    public class Hearing : BaseModel
    {
        [Key]
        public int HearingID { get; set; }
        [StringLength(120)]
        public string CaseId { get; set; } = string.Empty;
        public DateTime HearingDate { get; set; }
        public bool IsGoing { get; set; } = true;
        public bool IsPaid = false;
    }
}