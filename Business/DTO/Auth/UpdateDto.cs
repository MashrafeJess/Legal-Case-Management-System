using System;
using System.Collections.Generic;
using System.Text;

namespace Business.DTO.Auth
{
    public class UpdateDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int RoleId { get; set; }
    }
}