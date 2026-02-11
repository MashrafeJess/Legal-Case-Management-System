using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Database.Model
{
    public class FileEntity : BaseModel
    {
        [Key]
        [StringLength(128)]
        public string FileId { get; set; } = new Guid().ToString();

        [Required]
        [StringLength(100)]
        public string FileName { get; set; } = null!;  // Original name

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;     // Optional description

        public string FilePath { get; set; } = null!;      // Physical or cloud path
        public string ContentType { get; set; } = null!;   // MIME type, e.g., application/PDF
        public long Size { get; set; }

        public int CaseId { get; set; }

        public User CreatedByUser { get; set; } = null!;  // Navigation property to User who uploaded
    }
}