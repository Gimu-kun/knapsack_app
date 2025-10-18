using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using knapsack_app.ViewModels; // Cần thiết để sử dụng UserCreationReqDto
using System.Threading.Tasks; // Cần thiết cho async/await

namespace knapsack_app.Pages.Cli
{
    /// <summary>
    /// Model cho trang đăng ký người dùng (UserRegister.cshtml)
    /// </summary>
    public class RegisterModel : PageModel
    {
        private readonly UserService _userService; // Inject UserService
        
        // Cần inject UserService để gọi logic đăng ký
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
            // Reset ErrorMessage/SuccessMessage khi tải lại trang
            ErrorMessage = TempData["ErrorMessage"] as string ?? string.Empty;
            SuccessMessage = TempData["SuccessMessage"] as string ?? string.Empty;
        }

        /// <summary>
        /// Phương thức được gọi khi form được submit (gọi API đăng ký)
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            // 1. Kiểm tra Model State (dựa trên DataAnnotations)
            if (!ModelState.IsValid)
            {
                // Nếu validation thất bại, giữ lại trang và hiển thị lỗi
                return Page();
            }

            // 2. Tạo DTO từ InputModel
            var request = new UserCreationReqDto
            {
                Account = Input.Account,
                Passwords = Input.Passwords,
                Avatar = Input.Avatar // IFormFile được ánh xạ trực tiếp
            };

            // 3. Gọi Service để tạo User và lưu file
            var result = await _userService.CreateUser(request);

            if (result.Success)
            {
                // Đăng ký thành công
                TempData["SuccessMessage"] = "Đăng ký thành công! Bạn có thể đăng nhập ngay.";
                // Sau khi đăng ký thành công, chuyển hướng người dùng đến trang đăng nhập
                return RedirectToPage("/Cli/Login"); 
            }
            else
            {
                // Đăng ký thất bại (ví dụ: tài khoản đã tồn tại)
                TempData["ErrorMessage"] = result.Message;
                // Quay lại trang hiện tại để hiển thị lỗi (Post/Redirect/Get pattern không bắt buộc 
                // nhưng tốt hơn nếu không chuyển hướng)
                return Page();
            }
        }

        /// <summary>
        /// Class chứa các thuộc tính đầu vào (input) từ form Đăng ký.
        /// </summary>
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
