using Microsoft.AspNetCore.Mvc;
using knapsack_app.Services;

namespace knapsack_app.Controllers
{
    [ApiController]
    [Route("api/user/session_log")]
    public class SessionController : ControllerBase
    {
        private readonly SessionService _sessionService;

        public SessionController(SessionService sessionService)
        {
            _sessionService = sessionService;
        }

        [HttpGet("login/{userId}")]
        public async Task<IActionResult> Login(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User ID không được để trống!");

            var session = await _sessionService.SetLogin(userId);
            return Ok(new
            {
                Message = "Ghi nhận đăng nhập thành công!",
                Data = session
            });
        }

        [HttpGet("logout/{userId}")]
        public async Task<IActionResult> Logout(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User ID không được để trống!");

            var result = await _sessionService.SetLogout(userId);
            if (!result)
                return NotFound("Không tìm thấy session đang hoạt động!");

            return Ok(new { Message = "Ghi nhận đăng xuất thành công!" });
        }

        [HttpGet("history/{userId}")]
        public async Task<IActionResult> History(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User ID không được để trống!");

            var history = await _sessionService.GetSessionHistory(userId);
            return Ok(history);
        }
    }
}
