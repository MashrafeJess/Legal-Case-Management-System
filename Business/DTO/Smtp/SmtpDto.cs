using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Business.DTO.Smtp
{
    public  class SmtpDto
    {
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
