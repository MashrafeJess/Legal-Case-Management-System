using Business;
using Database.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication1.Pages.Account
{
    [AllowAnonymous]
    public class RegistrationModel : PageModel
    {
        [BindProperty]
        public new User? User { get; set; }

        public void OnGet()
        {
        }

        //public async Task<IActionResult> OnPostAsync()
        //{
        //    if (!ModelState.IsValid)
        //        return Page();

        //    var service = new UserService();
        //    Result result =  service.Registration(user);

        //    if (result.Success)
        //        return RedirectToPage("/Account/Login");

        //    ModelState.AddModelError(string.Empty, result.Message);
        //    return Page();
        //}
    }
}