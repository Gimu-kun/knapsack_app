
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
        private readonly GameService _gameService;
        private readonly ChallengeService _challengeService;

        public GameController(GameService gameService , ChallengeService challengeService)
        {
            _gameService = gameService;
            _challengeService = challengeService;
        }

        [HttpPost("Start")]
        public async Task<IActionResult> StartGame([FromBody] StartGameRequest request)
        {
            if (string.IsNullOrEmpty(request.ChallengeId) || string.IsNullOrEmpty(request.UserId))
            {
                return BadRequest(new { message = "ChallengeId và UserId không được để trống." });
            }

            try
            {
                ChallengeCreateEditModel challengeInfo = await _challengeService.GetChallengeByIdAsync(request.ChallengeId);
                if (challengeInfo == null)
                {
                    return NotFound(new { message = $"Challenge ID '{request.ChallengeId}' không tồn tại." });
                }

                var startTime = DateTime.UtcNow;

                var takenId = await _gameService.CreateNewTakenSession(request, challengeInfo, startTime);

                var maxDurationSeconds = challengeInfo.MaxDuration;
                var deadlineTime = startTime.AddSeconds(maxDurationSeconds);

                var response = new GameSessionStarted
                {
                    TakenId = takenId,
                    MaxDurationSeconds = maxDurationSeconds,
                    StartTimeUtc = startTime,
                    DeadlineUtc = deadlineTime
                };

                return Ok(new { success = true, takenId = response.TakenId, deadline = response.DeadlineUtc });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi hệ thống khi bắt đầu game: {ex.Message}" });
            }
        }

        [HttpGet("Status/{takenId}")]
        public async Task<IActionResult> GetGameStatus(string takenId)
        {
            if (string.IsNullOrEmpty(takenId))
            {
                return BadRequest(new { message = "Taken ID không được để trống." });
            }

            try
            {
                var status = await _gameService.GetGameStatus(takenId);

                if (status == null)
                {
                    return NotFound(new { message = $"Phiên chơi (Taken ID: {takenId}) không tồn tại." });
                }

                return Ok(new
                {
                    timeRemaining = status.TimeRemainingSeconds,
                    isTimeUp = status.IsTimeUp
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi hệ thống khi lấy trạng thái game: {ex.Message}" });
            }
        }

        [HttpPost("AdjustScore")]
        public async Task<IActionResult> AdjustScore([FromBody] AdjustScoreRequest request)
        {
            if (string.IsNullOrEmpty(request.TakenId))
            {
                return BadRequest(new { message = "Taken ID không được để trống." });
            }

            try
            {
                var response = await _gameService.AdjustTakenScore(request.TakenId, request.ScoreChange);

                if (!response.Success)
                {
                    return NotFound(new { message = response.Message });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi hệ thống khi điều chỉnh điểm: {ex.Message}" });
            }
        }
        
        [HttpPost("End/{TakenId}/{TakenTimeSeconds}")]
        public async Task<IActionResult> EndGame(string TakenId,int TakenTimeSeconds)
        {

            Console.WriteLine("API EndGame được gọi với TakenId: " + TakenId);
            Console.WriteLine("API EndGame được gọi với TakenTimeSeconds: " + TakenTimeSeconds);
            if (string.IsNullOrEmpty(TakenId))
            {
                return BadRequest(new { message = "Taken ID không được để trống." });
            }

            try
            {
                var response = await _gameService.EndGame(TakenId,TakenTimeSeconds);

                if (!response.Success)
                {
                    return NotFound(new { message = response.Message });
                }
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi hệ thống kết thúc game: {ex.Message}" });
            }
        }
    }
}