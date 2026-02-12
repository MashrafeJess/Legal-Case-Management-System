using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Business.DTO.CaseDto
{
    public class CaseDto
    {
        public int CaseId { get; set; }
        public string Email { get; set; } = string.Empty;
        public int CaseType { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string CaseName { get; set; } = string.Empty;
        public string CaseHandlingBy { get; set; } = string.Empty;
        public int Fee { get; set; }
        public ICollection<IFormFile>? FormFiles { get; set; }
    }
}