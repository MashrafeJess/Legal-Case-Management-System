using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Database.Enums;

namespace Database.Model
{
    public class Comment : BaseModel
    {
        [Key]
        public int CommentId { get; set; }

        [StringLength(250)]
        public string CommentText { get; set; } = string.Empty;

        [StringLength(120)]
        public string UserId { get; set; } = string.Empty;

        public int CaseId { get; set; }
        public string ?CaseStatus { get; set; } = string.Empty;
    }
}