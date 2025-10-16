using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace knapsack_app.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet() { }
    
    public IActionResult OnPostLogout()
    {
        HttpContext.Session.Remove("JwtToken");
        HttpContext.Session.Remove("uid");
        return RedirectToPage("/Index");
    }
}