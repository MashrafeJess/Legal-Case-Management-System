using Business;
using Database.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication1.Pages.Account
{
    public class LawyerListModel : PageModel
    {
        public List<User> List { get; set; } = [];
        //public void OnGet()
        //{
        //    Result results = new UserService().GetAllLawyers();
        //    if (results.Success)
        //    {
        //        List = results.Data as List<User>;
        //    }
        //}
    }
}