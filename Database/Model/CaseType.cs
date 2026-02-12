using System.ComponentModel.DataAnnotations;

namespace Database.Model
{
    public class CaseType : BaseModel
    {
        [Key]
        public int CaseTypeId { get; set; }
        public string CaseTypeName { get; set; } = string.Empty;
        public string CaseTypeDescription { get; set; } = string.Empty;
    }
}