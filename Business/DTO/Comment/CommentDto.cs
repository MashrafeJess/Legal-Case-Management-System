using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Business.DTO.Comment
{
    public class CommentDto
    {
        public string CommentText { get; set; } = string.Empty;

        public string UserId { get; set; } = string.Empty;

        public int CaseId { get; set; }
    }
}