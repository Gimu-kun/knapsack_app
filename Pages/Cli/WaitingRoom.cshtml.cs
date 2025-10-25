using System.ComponentModel.DataAnnotations;
using knapsack_app.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Threading.Tasks;

namespace knapsack_app.Pages.Cli
{
    public class WaitingRoomModel : PageModel
    {
        private readonly JwtService _jwtService;
        private readonly UserService _userService;
        public UserModel CurrentUser { get; set; } = null;
        [BindProperty(SupportsGet = true)]
        public string Id { get; set; } 

        public WaitingRoomModel(JwtService jwtService, UserService userService)
        {
            _jwtService = jwtService;
            _userService = userService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(Id))
            {
                return RedirectToPage("/selection");
            }

            if (HttpContext.Request.Cookies.TryGetValue("UserToken", out var token) && !string.IsNullOrEmpty(token))
            {
                var principal = _jwtService.ValidateToken(token);
                
                if (principal != null)
                {
                    var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId))
                    {
                        CurrentUser = await _userService.GetUserById(userId);

                        if (CurrentUser == null)
                        {
                            return RedirectToPage("/login");
                        }
                        
                        return Page();
                    }
                }
            }
            
            return RedirectToPage("/login"); 
        }
    }
}