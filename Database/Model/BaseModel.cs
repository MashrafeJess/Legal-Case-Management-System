using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Model
{
    public class BaseModel
    {
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        [StringLength(120)]
        public string? CreatedBy { get; set; } = string.Empty;

        [StringLength(120)]
        public string? UpdatedBy { get; set; } = string.Empty;

        public bool IsDeleted { get; set; } = false;
    }
}