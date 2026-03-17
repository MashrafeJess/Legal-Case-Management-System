using System.ComponentModel.DataAnnotations;

namespace Database.Model
{
    public class Salary : BaseModel
    {
        [Key]
        public int SalaryId { get; set; }

        public int Amount { get; set; }
    }
}