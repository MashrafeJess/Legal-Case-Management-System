using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Database;
using Business;
using Microsoft.AspNetCore.Mvc;
using Database.Model;

namespace WebApplication1.Pages.Account
{
    [Authorize]
    public class DashboardModel : PageModel
    {
        public string? IsLoggedInUser { get; set; }
        public User? CurrentUser { get; set; }

        //public async Task<IActionResult> OnGetAsync()
        //{
        //    // Get current user ID
        //    IsLoggedInUser = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //    if (string.IsNullOrEmpty(IsLoggedInUser))
        //    {
        //        return RedirectToPage("/Account/Login");
        //    }

        //    Get user details from service
        //   Result result = new UserService().UserById(IsLoggedInUser);

        //    if (result.Data == null || !result.Success)
        //    {
        //        return RedirectToPage("/Account/Unauthorized");
        //    }

        //    CurrentUser = result.Data as User;
        //    return Page();
        //}
    }
}