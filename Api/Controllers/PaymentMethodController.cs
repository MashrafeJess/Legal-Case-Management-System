using Business.DTO.PaymentMethod;
using Business.Services;
using Database.Model;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentMethodController(PaymentMethodService service) : Controller
    {
        private readonly PaymentMethodService _service = service;

        [HttpPost("create")]
        public async Task<IActionResult> Create(MethodDto method)
        {
            var result = await _service.AddPaymentMethod(method);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update(PaymentMethod method)
        {
            if (method == null)
                return BadRequest("CaseDto was Null");
            var result = await _service.UpdatePaymentMethod(method);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("alltype")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.AllPaymentMethods();
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("getById")]
        public async Task<IActionResult> GetById(int methodId)
        {
            var result = await _service.PaymentMethodById(methodId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(int methodId)
        {
            var result = await _service.DeletePaymentMethod(methodId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}