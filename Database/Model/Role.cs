using System.ComponentModel.DataAnnotations;

namespace Database.Model
{
    public class Role : BaseModel
    {
        [Key]
        public int RoleId { get; set; }

        public string RoleName { get; set; } = string.Empty;
    }
}