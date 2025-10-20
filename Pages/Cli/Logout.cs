// Pages/Auth/Logout.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace knapsack_app.Pages.Auth
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnPost()
        {
            HttpContext.Response.Cookies.Delete("UserToken");
            HttpContext.Response.Cookies.Delete("UserDisplayInfo");
            return RedirectToPage("/login");
        }
    }
}
