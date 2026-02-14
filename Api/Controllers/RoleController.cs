using Business.DTO.Role;
using Business.Services;
using Database.Model;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController(RoleService service) : Controller
    {
        private readonly RoleService _service = service;

        [HttpPost("create")]
        public async Task<IActionResult> Create(RoleDto role)
        {
            var result = await _service.AddRole(role);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update(Role role)
        {
            if (role == null)
                return BadRequest("CaseDto was Null");
            var result = await _service.UpdateRole(role);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.AllRoles();
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("getById/{roleId}")]
        public async Task<IActionResult> GetById(int roleId)
        {
            var result = await _service.RoleById(roleId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("delete/{roleId}")]
        public async Task<IActionResult> Delete(int roleId)
        {
            var result = await _service.DeleteRole(roleId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}