using System.ComponentModel.DataAnnotations;

namespace Database.Model
{
    public class SmtpSettings : BaseModel
    {
        [Key]
        public int SmtpId { get; set; }
        [StringLength(50)]
        public string Host { get; set; } = "";
        public int Port { get; set; }
        [StringLength(50)]
        public string Username { get; set; } = "";
        [StringLength(50)]
        public string Password { get; set; } = "";
        public bool EnableSsl { get; set; } = true;
        [StringLength(100)]
        public string SenderEmail { get; set; } = "";
    }
}