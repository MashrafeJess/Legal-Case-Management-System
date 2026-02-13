namespace Business.DTO.Mail
{
    public class SendMailDto
    {
        public string ReceiverUserId { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }

    public class MailResponseDto
    {
        public string SenderName { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}