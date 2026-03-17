using System.Security.Claims;
using Business.DTO.NOC;
using Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NOCController(NOCService service) : ControllerBase
    {
        private readonly NOCService _service = service;

        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] ApplyNOCDto dto)
        {
            var result = await _service.ApplyAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("approve")]
        [Authorize(Roles = "Admin,Lawyer")]
        public async Task<IActionResult> Approve([FromBody] ApproveNOCDto dto)
        {
            var result = await _service.ApproveAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("reject")]
        [Authorize(Roles = "Admin,Lawyer")]
        public async Task<IActionResult> Reject([FromBody] RejectNOCDto dto)
        {
            var result = await _service.RejectAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var role = User.FindFirstValue(ClaimTypes.Role)!;
            var result = await _service.GetAllAsync(userId, role);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("by-case/{caseId}")]
        public async Task<IActionResult> GetByCaseId(int caseId)
        {
            var result = await _service.GetByCaseIdAsync(caseId);
            return result.Success ? Ok(result) : NotFound(result);
        }
    }
}