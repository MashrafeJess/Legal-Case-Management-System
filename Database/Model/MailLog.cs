using System.ComponentModel.DataAnnotations;

namespace Database.Model
{
    public class MailLog : BaseModel
    {
        [Key]
        public int MailLogId { get; set; }

        public string SenderUserId { get; set; } = string.Empty;
        public string ReceiverUserId { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User? Sender { get; set; }
        public User? Receiver { get; set; }
    }
}