using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Model
{
    public class Role : BaseModel
    {
        [Key]
        public int RoleId { get; set; }

        public string RoleName { get; set; } = string.Empty;
    }
}