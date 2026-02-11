using System.Security.Claims;
using Business.DTO.AuthDto;
using Business.Services;
using Database.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class UserController(UserService userService, IHttpContextAccessor httpContextAccessor) : Controller
    {
        private readonly UserService _userService = userService;
        private readonly IHttpContextAccessor _accessor = httpContextAccessor;

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Registration(RegistrationDto user)
        {
            var result = await _userService.RegisterAsync(user);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto user)
        {
            var result = await _userService.Login(user);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(string userId)
        {
            var result = await _userService.Delete(userId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateDto user)
        {
            string? UserId = _accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _userService.Update(user, UserId!);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("GetAll")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> GetUserList()
        {
            var result = await _userService.AllUsers();
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("Single")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            var result = await _userService.UserById(userId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}