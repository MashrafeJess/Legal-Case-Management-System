using System.ComponentModel.DataAnnotations;

namespace Database.Model
{
    public class Token
    {
        [Key]
        public int Id { get; set; }
        [StringLength(128)]
        public string TokenId { get; set; } = string.Empty;
        [StringLength(100)]
        public string Email { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public TimeSpan Delay { get; set; } = TimeSpan.FromMinutes(3);
        public bool IsExpired { get; set; } = false;
    }
}