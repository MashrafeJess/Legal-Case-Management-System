using Business.DTO.CaseDto;
using Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class CaseController(CaseService service) : Controller
    {
        private readonly CaseService _service = service;

        [HttpPost("case/create")]
        public async Task<IActionResult> CreateCase(CaseDto cases)
        {
            var result = await _service.AddCase(cases);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("case/update")]
        public async Task<IActionResult> UpdateCase(CaseDto cases)
        {
            if (cases == null)
                return BadRequest("CaseDto was Null");
            var result = await _service.UpdateCase(cases);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("case/AllCase")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.AllCases();
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("case/getById")]
        public async Task<IActionResult> GetById( int caseId)
        {
            var result = await _service.CaseById(caseId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("case/delete")]
        public async Task<IActionResult> Delete( int caseId)
        {
            var result = await _service.DeleteCase(caseId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}