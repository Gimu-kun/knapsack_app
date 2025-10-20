// File: ViewComponents/HeaderViewComponent.cs

using knapsack_app.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
// Thêm namespace cho DTO và Service của bạn

public class HeaderViewComponent : ViewComponent
{
    private readonly JwtService _jwtService;
    private readonly UserService _userService;

    public HeaderViewComponent(JwtService jwtService, UserService userService)
    {
        _jwtService = jwtService;
        _userService = userService;
    }

    // Phương thức này sẽ chạy khi component được gọi
    public async Task<IViewComponentResult> InvokeAsync()
    {
        UserModel currentUser = null;
        
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
                    currentUser =  await _userService.GetUserById(userId);
                }
            }
        }

        // Truyền đối tượng UserDto (hoặc null) sang View Component Razor file
        return View(currentUser);
    }
}