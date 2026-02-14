using Business.DTO.Comment;
using Business.Services;
using Database.Model;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController(CommentService service) : Controller
    {
        private readonly CommentService _service = service;

        [HttpPost("create")]
        public async Task<IActionResult> Create(CommentDto comment)
        {
            var result = await _service.Create(comment);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update(Comment comment)
        {
            if (comment == null)
                return BadRequest("CaseDto was Null");
            var result = await _service.Update(comment);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("alltype")]
        public async Task<IActionResult> GetAll(int caseId)
        {
            var result = await _service.Get(caseId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("getById")]
        public async Task<IActionResult> GetById(int commentId)
        {
            var result = await _service.GetById(commentId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(int commentId)
        {
            var result = await _service.Delete(commentId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}