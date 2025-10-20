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

        // Thuộc tính để lưu trữ thông tin người dùng và truyền ra View
        public UserModel CurrentUser { get; set; } = null;
        
        // Thuộc tính để lấy ID phòng từ Query String
        [BindProperty(SupportsGet = true)]
        public string Id { get; set; } // Sẽ nhận giá trị từ ?id=R-XXXXX

        public WaitingRoomModel(JwtService jwtService, UserService userService)
        {
            _jwtService = jwtService;
            _userService = userService;
        }

        // Dùng OnGetAsync để thực hiện logic khi trang được load
        public async Task<IActionResult> OnGetAsync()
        {
            // Kiểm tra ID phòng
            if (string.IsNullOrEmpty(Id))
            {
                // Nếu không có ID phòng, chuyển hướng về trang chọn
                return RedirectToPage("/selection");
            }

            // 1. Lấy JWT từ cookie "UserToken"
            if (HttpContext.Request.Cookies.TryGetValue("UserToken", out var token) && !string.IsNullOrEmpty(token))
            {
                // 2. Xác thực token
                var principal = _jwtService.ValidateToken(token);
                
                if (principal != null)
                {
                    // 3. Lấy UserId từ claim
                    var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    
                    if (!string.IsNullOrEmpty(userId))
                    {
                        // 4. Lấy thông tin người dùng từ DB
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