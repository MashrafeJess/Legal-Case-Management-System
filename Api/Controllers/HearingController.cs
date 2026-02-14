using Business.DTO.Hearing;
using Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HearingController(HearingService service) : ControllerBase
    {
        private readonly HearingService _service = service;

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] CreateHearingDto dto)
        {
            var result = await _service.AddHearing(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UpdateHearingDto dto)
        {
            var result = await _service.UpdateHearing(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("delete/{hearingId}")]
        public async Task<IActionResult> Delete(int hearingId)
        {
            var result = await _service.DeleteHearing(hearingId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("all")]
        public async Task<IActionResult> All()
        {
            var result = await _service.AllHearings();
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("{hearingId}")]
        public async Task<IActionResult> GetById(int hearingId)
        {
            var result = await _service.HearingById(hearingId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        // Manually trigger reminder — useful for testing
        [HttpPost("send-reminders")]
        public async Task<IActionResult> SendReminders()
        {
            var result = await _service.SendPendingCommentRemindersAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}