namespace Business.DTO.Auth
{
    public class VerifyDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class ResendOtpDto
    {
        public string UserId { get; set; } = string.Empty;
    }
}