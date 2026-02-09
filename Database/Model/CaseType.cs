using System;
using System.Collections.Generic;
using System.Text;

namespace Database.Model
{
    public class CaseType : BaseModel
    {
        public int CaseTypeId { get; set; }
        public string CaseTypeDescription { get; set; } = string.Empty;
        public int Price { get; set; }
    }
}
