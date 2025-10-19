// File: knapsack_app.Controllers/GameController.cs
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using knapsack_app.ViewModels;
using knapsack_app.Services;

namespace knapsack_app.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        // Giả lập Service để thao tác với CSDL (ví dụ: lấy Challenge Data, lưu Taken)
        private readonly GameService _gameService;
        private readonly ChallengeService _challengeService;

        // Giả định bạn đã inject IGameService (hoặc tương đương)
        public GameController(GameService gameService , ChallengeService challengeService) // Thay đổi constructor để nhận IGameService
        {
            _gameService = gameService;
            _challengeService = challengeService;
        }

        /**
         * API: POST api/Game/Start
         * Mục đích: Lưu phiên chơi mới (bắt đầu tính thời gian) và trả về thông tin thời gian.
         */
        [HttpPost("Start")]
        public async Task<IActionResult> StartGame([FromBody] StartGameRequest request)
        {
            // Cần đảm bảo DTO StartGameRequest (chưa có trong file đính kèm) có các thuộc tính này
            if (string.IsNullOrEmpty(request.ChallengeId) || string.IsNullOrEmpty(request.UserId))
            {
                // Sử dụng BadRequest khi ModelState không hợp lệ hoặc thiếu dữ liệu bắt buộc
                return BadRequest(new { message = "ChallengeId và UserId không được để trống." });
            }

            try
            {

                Console.WriteLine("ID nè--------------" + request.ChallengeId);
                // 1. Lấy thông tin MaxDuration từ Challenge
                ChallengeCreateEditModel challengeInfo = await _challengeService.GetChallengeByIdAsync(request.ChallengeId);
                if (challengeInfo == null)
                {
                    return NotFound(new { message = $"Challenge ID '{request.ChallengeId}' không tồn tại." });
                }

                // Lấy thời điểm Server bắt đầu phiên chơi (rất quan trọng)
                var startTime = DateTime.UtcNow; 
                
                // 2. Lưu phiên chơi vào bảng TAKEN
                var takenId = await _gameService.CreateNewTakenSession(request, challengeInfo, startTime);

                // 3. Tính toán thời điểm hết hạn (Deadline)
                var maxDurationSeconds = challengeInfo.MaxDuration;
                var deadlineTime = startTime.AddSeconds(maxDurationSeconds);

                // 4. Trả về thông tin cần thiết cho Front-end
                var response = new GameSessionStarted
                {
                    TakenId = takenId,
                    MaxDurationSeconds = maxDurationSeconds,
                    StartTimeUtc = startTime,
                    DeadlineUtc = deadlineTime
                };
                
                // Trả về JSON với format { takenId: "...", deadline: "2025-10-20T10:00:00.000Z", ...}
                // Thêm trường success để Front-end dễ kiểm tra
                return Ok(new { success = true, takenId = response.TakenId, deadline = response.DeadlineUtc });
            }
            catch (Exception ex)
            {
                // Log lỗi
                return StatusCode(500, new { message = $"Lỗi hệ thống khi bắt đầu game: {ex.Message}" });
            }
        }
    }
}