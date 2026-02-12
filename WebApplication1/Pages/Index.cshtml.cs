using Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Business;

namespace WebApplication1.Pages
{
    [AllowAnonymous]
    public class IndexModel() : PageModel
    {
        public List<string>? Roles { get; set; }

        public void OnGet()
        {
            Roles = ["1", "2", "3"];
            //Result result = new Top10Service().ListTop10Events();
            //top = result.Data as List<Top10Images> ?? new List<Top10Images>();
        }
    }
}