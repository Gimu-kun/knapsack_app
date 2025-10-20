using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace knapsack_app.Pages.Cli
{
    public class LoginModel : PageModel
    {
        private readonly UserService _userService; 
        
        public LoginModel(UserService userService)
        {
            _userService = userService;
        }

        public class LoginInput
        {
            [Required(ErrorMessage = "Vui lòng nhập Tài khoản.")]
            [Display(Name = "Tài khoản")]
            public string Account { get; set; } = string.Empty; 

            [Required(ErrorMessage = "Vui lòng nhập Mật khẩu.")]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; } = string.Empty;
        }

        [BindProperty]
        public LoginInput Input { get; set; } = new LoginInput();
        
        [TempData]
        public string ErrorMessage { get; set; } = string.Empty;


        public void OnGet()
        {
            ErrorMessage = TempData["ErrorMessage"] as string ?? string.Empty;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var request = new LoginReqDto
            {
                Account = Input.Account,
                Password = Input.Password
            };

            var authResult = await _userService.Login(request);

            if (authResult.Success)
            {
                if (authResult.Token != null)
                {
                    HttpContext.Response.Cookies.Append("UserToken", authResult.Token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddHours(6)
                    });
                }
                
                return RedirectToPage("/Cli/Home"); 
            }
            else
            {
                ErrorMessage = authResult.Message;
                Input.Password = string.Empty; 
                return Page();
            }
        }
    }
}
