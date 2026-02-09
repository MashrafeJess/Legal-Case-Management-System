using System.ComponentModel.DataAnnotations;

namespace Database.Model
{
    public class BaseModel
    {
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        [StringLength(120)]
        public string? CreatedBy { get; set; } = Guid.NewGuid().ToString();
        [StringLength(120)]
        public string? UpdatedBy { get; set; } = Guid.NewGuid().ToString();
        public bool IsDeleted { get; set; } = false;
    }
}
