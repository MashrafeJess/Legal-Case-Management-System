namespace Database.Model
{
    public class CaseType : BaseModel
    {
        public int CaseTypeId { get; set; }
        public string CaseTypeDescription { get; set; } = string.Empty;
    }
}