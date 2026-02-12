using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Business;
using Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Database.Model;

namespace WebApplication1.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        [BindProperty]
        public new User? User { get; set; }

        public void OnGet()
        {
        }

        //public async Task<IActionResult> OnPostAsync()
        //{
        //    Result result =  new UserService().Login(user);
        //    if (result.Success)
        //    {
        //        User user = result.Data as User;
        //        var claims = new List<Claim>
        //        {
        //            new(ClaimTypes.NameIdentifier, user.UserId),
        //            new(ClaimTypes.Name,user.UserName),
        //            new(ClaimTypes.Role,user.RoleId.ToString())
        //        };

        //        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        //        var principal = new ClaimsPrincipal(identity);
        //        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        //        return RedirectToPage("/Account/Dashboard");
        //    }
        //    else return Page();
        //}

        public async Task<IActionResult> OnGetLogoutAsync()
        {
            await HttpContext.SignOutAsync();
            return RedirectToPage("/Account/Login");
        }
    }
}