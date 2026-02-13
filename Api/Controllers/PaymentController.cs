using Business.DTO.Payment;
using Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Business.DTO.Payment.PaymentDto;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController(PaymentService service) : ControllerBase
    {
        private readonly PaymentService _service = service;

        // Frontend calls this to start payment
        [HttpPost("initiate")]
        [Authorize]
        public async Task<IActionResult> Initiate([FromBody] InitiatePaymentDto dto)
        {
            var result = await _service.InitiatePaymentAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // SSLCommerz POSTs here on success
        [HttpPost("success")]
        [AllowAnonymous]
        public async Task<IActionResult> Success([FromForm] string tran_id, [FromForm] string val_id)
        {
            var result = await _service.PaymentSuccessAsync(tran_id, val_id);

            return result.Success
                ? Redirect("https://yourfrontend.com/payment/success")
                : Redirect("https://yourfrontend.com/payment/failed");
        }

        // SSLCommerz POSTs here on fail
        [HttpPost("fail")]
        [AllowAnonymous]
        public async Task<IActionResult> Fail([FromForm] string tran_id)
        {
            await _service.PaymentFailAsync(tran_id);
            return Redirect("https://yourfrontend.com/payment/failed");
        }

        // SSLCommerz POSTs here on cancel
        [HttpPost("cancel")]
        [AllowAnonymous]
        public async Task<IActionResult> Cancel([FromForm] string tran_id)
        {
            await _service.PaymentCancelAsync(tran_id);
            return Redirect("https://yourfrontend.com/payment/cancelled");
        }

        // Get all payments for a case
        [HttpGet("case/{caseId}")]
        [Authorize]
        public async Task<IActionResult> GetByCaseId(int caseId)
        {
            var result = await _service.GetPaymentsByCaseAsync(caseId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        // Get single payment
        [HttpGet("{paymentId}")]
        [Authorize]
        public async Task<IActionResult> GetById(string paymentId)
        {
            var result = await _service.GetPaymentByIdAsync(paymentId);
            return result.Success ? Ok(result) : NotFound(result);
        }
    }
}