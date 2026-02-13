using System.Security.Claims;
using Business.Services;
using Database.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class FileController(FileService service, IHttpContextAccessor accessor) : Controller
    {
        private readonly FileService _service = service;
        private readonly IHttpContextAccessor _accessor = accessor;

        [HttpPost("file/create")]
        public async Task<IActionResult> Create(IFormFile file, int caseId)
        {
            string? userId = _accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _service.UploadAsync(file, userId!, caseId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("file/delete")]
        public async Task<IActionResult> Delete(string fileId)
        {
            var result = await _service.DeleteFileAsync(fileId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("file/getallbycaseid")]
        public async Task<IActionResult> Get(int Id)
        {
            var result = await _service.GetFilesAsync(Id);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("file/getbyid")]
        public async Task<IActionResult> GetById(string Id)
        {
            var result = await _service.FileById(Id);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("download/{fileId}")]
        public async Task<IActionResult> DownloadFile(string fileId)
        {
            var result = await _service.DownloadAsync(fileId);
            if (!result.Success)
                return NotFound(result.Message);

            if (result.Data is not FileEntity file)
                return StatusCode(500, "Invalid file data");

            var bytes = await System.IO.File.ReadAllBytesAsync(file.FilePath);
            return File(bytes, file.ContentType, file.FileName);
        }
    }
}