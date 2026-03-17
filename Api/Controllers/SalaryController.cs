using Database.Context;
using Database.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Utilities.Zlib;
using static System.Net.Mime.MediaTypeNames;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class SalaryController(LMSContext context) : ControllerBase
    {
        private readonly LMSContext _context = context;

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var salaries = await _context.Salary
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.Amount)
                .ToListAsync();

            return Ok(new { success = true, data = salaries });
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] Salary salary)
        {
            _context.Salary.Add(salary);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Salary added" });
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] Salary salary)
        {
            _context.Salary.Update(salary);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Salary updated" });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var salary = await _context.Salary.FindAsync(id);
            if (salary == null)
                return NotFound(new { success = false, message = "Not found" });

            salary.IsDeleted = true;
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Salary deleted" });
        }
    }
}