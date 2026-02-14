using Business.Services;
using Database.Model;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TypeController(CaseTypeService service) : Controller
    {
        private readonly CaseTypeService _service = service;

        [HttpPost("create")]
        public async Task<IActionResult> Create(CaseType type)
        {
            var result = await _service.Create(type);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update(CaseType type)
        {
            if (type == null)
                return BadRequest("CaseDto was Null");
            var result = await _service.Update(type);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("alltype")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAll();
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("getById")]
        public async Task<IActionResult> GetById(int typeId)
        {
            var result = await _service.GetById(typeId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(int typeId)
        {
            var result = await _service.Delete(typeId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}