using knapsack_app.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class HeaderViewComponent : ViewComponent
{
    private readonly JwtService _jwtService;
    private readonly UserService _userService;

    public HeaderViewComponent(JwtService jwtService, UserService userService)
    {
        _jwtService = jwtService;
        _userService = userService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        UserModel currentUser = null;
        
        if (HttpContext.Request.Cookies.TryGetValue("UserToken", out var token) && !string.IsNullOrEmpty(token))
        {
            var principal = _jwtService.ValidateToken(token);
            if (principal != null)
            {
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    currentUser =  await _userService.GetUserById(userId);
                }
            }
        }

        return View(currentUser);
    }
}