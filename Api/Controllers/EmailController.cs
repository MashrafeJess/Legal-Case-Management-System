using Business;
using Business.DTO.Mail;
using Business.DTO.Smtp;
using Business.Services;
using Database;
using Database.Model;
using MailKit;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailSendController(SmtpService service, EmailService emailService) : Controller
    {
        private readonly SmtpService _service = service;
        private readonly EmailService _emailService = emailService;

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromQuery] string toEmail)
        {
            var result = await _emailService.SendOtpEmailAsync(toEmail);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMail([FromBody] SendMailDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.ReceiverUserId))
                return BadRequest(new Result(false, "ReceiverUserId is required"));

            if (string.IsNullOrWhiteSpace(dto.Subject))
                return BadRequest(new Result(false, "Subject is required"));

            if (string.IsNullOrWhiteSpace(dto.Body))
                return BadRequest(new Result(false, "Body is required"));

            var result = await _emailService.SendMailToUserAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("create-smtp")]
        public async Task<IActionResult> CreateSmtp(SmtpDto smtpConfig)
        {
            var result = await _service.AddSmtp(smtpConfig);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result.Message);
            }
        }

        [HttpPost("update-smtp")]
        public async Task<IActionResult> UpdateSmtp(SmtpSettings smtpConfig)
        {
            var result = await _service.UpdateSmtp(smtpConfig);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result.Message);
            }
        }

        [HttpGet("getById")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _service.GetSmtp(id);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result.Message);
            }
        }
    }
}