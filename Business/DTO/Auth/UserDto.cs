namespace Business.DTO.Auth
{
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class UpdateProfileDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public class AdminUpdateUserDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int RoleId { get; set; }
    }
}