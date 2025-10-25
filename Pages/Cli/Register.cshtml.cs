using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace knapsack_app.Pages.Cli
{
    public class RegisterModel : PageModel
    {
        private readonly UserService _userService; 
        
        public RegisterModel(UserService userService)
        {
            _userService = userService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        [TempData]
        public string ErrorMessage { get; set; } = string.Empty;
        
        [TempData]
        public string SuccessMessage { get; set; } = string.Empty;

        public void OnGet()
        {
            ErrorMessage = TempData["ErrorMessage"] as string ?? string.Empty;
            SuccessMessage = TempData["SuccessMessage"] as string ?? string.Empty;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var request = new UserCreationReqDto
            {
                Account = Input.Account,
                Passwords = Input.Passwords,
                Avatar = Input.Avatar
            };

            var result = await _userService.CreateUser(request);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Đăng ký thành công! Bạn có thể đăng nhập ngay.";
                return RedirectToPage("/Cli/Login"); 
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
                return Page();
            }
        }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập Tài khoản.")]
            [StringLength(50, ErrorMessage = "Tài khoản không được quá 50 ký tự.")]
            [Display(Name = "Tài khoản")]
            public string Account { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập Mật khẩu.")]
            [DataType(DataType.Password)]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải dài từ 6 đến 100 ký tự.")]
            [Display(Name = "Mật khẩu")]
            public string Passwords { get; set; } = string.Empty;

            [Display(Name = "Ảnh đại diện")]
            public IFormFile? Avatar { get; set; }
        }
    }
}
