using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Database.Model
{
    public class Case : BaseModel
    {
        [Key]
        [StringLength(120)]
        [Required]
        public int CaseId { get; set; } 
        [Required]
        public string UserId { get; set; } = string.Empty;
        [StringLength(50)]
        public string CaseName { get; set; } = String.Empty;
        public int CaseType { get; set; } 
        [StringLength(120)]
        public string CaseHandlingBy { get; set; } = string.Empty;
        public int HearingNumber { get; set; }
        public int Price { get; set; }
    }
}
