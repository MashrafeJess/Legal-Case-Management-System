using Business.DTO.Report;
using Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ReportController(ReportService service) : ControllerBase
    {
        private readonly ReportService _service = service;

        [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthlyReport(
            [FromQuery] int year,
            [FromQuery] int month)
        {
            var result = await _service.GetMonthlyReportAsync(year, month);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("yearly")]
        public async Task<IActionResult> GetYearlyReport([FromQuery] int year)
        {
            var result = await _service.GetYearlyReportsAsync(year);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}