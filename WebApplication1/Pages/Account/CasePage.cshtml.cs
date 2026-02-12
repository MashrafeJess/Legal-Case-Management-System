using System.Security.Claims;
using Business;
using Database.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplication1.Pages.Account
{
    public class CasePageModel : PageModel
    {
        [BindProperty]
        public Case Cases { get; set; } = new Case();

        // Raw lawyers list (optional to keep)
        public List<User> Lawyers { get; set; } = [];

        // ✅ Dropdown options for the UI
        public IEnumerable<SelectListItem> LawyerOptions { get; set; } = [];

        //public void OnGet(int? Id = null)
        //{
        //    if (Id != null)
        //    {
        //        //Result result = new CaseService().CaseById(Id.Value);
        //        cases = result.Data as Case ?? new Case();
        //    }

        //    LoadLawyersDropdown();
        //}

        //public IActionResult OnPost()
        //{
        //    // ✅ reload dropdown on postback (otherwise dropdown breaks on validation errors)
        //    LoadLawyersDropdown();

        //    cases.CreatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //    Result result;

        //    if (cases.CaseId == 0)
        //    {
        //        result = new CaseService().AddCase(cases);
        //    }
        //    else
        //    {
        //        cases.UpdatedDate = DateTime.Now;
        //        cases.UpdatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        result = new CaseService().UpdateCase(cases);
        //    }

        //    if (result.Success)
        //        return RedirectToPage("/Account/CaseList");

        //    return Page();
        //}

        //private void LoadLawyersDropdown()
        //{
        //    var result1 = new UserService().GetAllLawyers();

        //    // ✅ Always validate Result first
        //    if (result1.Success && result1.Data is List<User> list)
        //    {
        //        Lawyers = list;

        //        LawyerOptions = Lawyers.Select(l => new SelectListItem
        //        {
        //            Value = l.UserId,      // saved value
        //            Text = l.UserName      // displayed text
        //        }).ToList();
        //    }
        //    else
        //    {
        //        Lawyers = new List<User>();
        //        LawyerOptions = Enumerable.Empty<SelectListItem>();
        //    }
        //}
    }
}