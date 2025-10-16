using Microsoft.AspNetCore.Mvc;
using knapsack_app.Models.Models;
using knapsack_app.Services;


namespace knapsack_app.Controllers
{
    [ApiController]
    [Route("api/admin/history")]
    public class HistoryController : ControllerBase
    {
        private readonly HistoryService _historyService;

        public HistoryController(HistoryService historyService)
        {
            _historyService = historyService;
        }
        
        [HttpGet("{userId}")]
        public async Task<ActionResult<IEnumerable<PlayHistoryApiViewModel>>> GetUserPlayHistory(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { Message = "User ID không được để trống." });
            }

            try
            {
                var history = await _historyService.GetUserPlayHistoryAsync(userId);
                return Ok(history);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API ERROR] Lỗi khi tải lịch sử chơi cho User {userId}: {ex.Message}");
                return StatusCode(500, new { Message = "Đã xảy ra lỗi server khi tải dữ liệu." });
            }
        }
    }
}