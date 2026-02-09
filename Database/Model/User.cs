using System.ComponentModel.DataAnnotations;

namespace Database.Model
{
    public class User : BaseModel
    {
        [Key]
        [StringLength(50)]
        public string UserId { get; set; } = Guid.NewGuid().ToString();
        [StringLength(120)]
        public string UserName { get; set; } = string.Empty;
        [Required(ErrorMessage = "password field can't be empty")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty.ToString();
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [StringLength(100)]
        public string ?Address { get; set; } = string.Empty;
        public int RoleId { get; set; }
    }
}
