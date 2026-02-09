using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Database.Model
{
    public class Comment : BaseModel
    {
        [Key]
        public string CommentId { get; set; } = Guid.NewGuid().ToString();
        [StringLength(250)]
        public string CommentText { get; set; } = string.Empty;
        [StringLength(120)]
        public string UserId { get; set; } = string.Empty;
        [StringLength(120)]
        public string CaseId { get; set; } = string.Empty;
    }
}
