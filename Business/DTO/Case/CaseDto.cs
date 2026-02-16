using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Business.DTO.Case
{
    public class CreateCaseDto
    {
        public string Email { get; set; } = string.Empty;
        public int CaseTypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string CaseName { get; set; } = string.Empty;
        public string CaseHandlingBy { get; set; } = string.Empty;
        public int Fee { get; set; }
        public ICollection<IFormFile>? FormFiles { get; set; }
    }

    public class CaseDto
    {
        public int CaseId { get; set; }
        public string Email { get; set; } = string.Empty;
        public int CaseTypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string CaseName { get; set; } = string.Empty;
        public string CaseHandlingBy { get; set; } = string.Empty;
        public int Fee { get; set; }
        public bool IsConsultationFeePaid { get; set; }
        public ICollection<IFormFile>? FormFiles { get; set; }
    }
}