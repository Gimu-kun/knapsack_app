using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using knapsack_app.Services;

namespace knapsack_app.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class AnalyzingController : ControllerBase
    {
        private readonly AnalyzingService _analyzingService;

        public AnalyzingController(AnalyzingService analyzingService)
        {
            _analyzingService = analyzingService;
        }

        [HttpGet("difficulty-stats")]
        [ProducesResponseType(typeof(List<DailyChallengeStatDto>), 200)]
        public async Task<IActionResult> GetDifficultyStats()
        {
            var stats = await _analyzingService.GetDailyTakenStatsAsync();
            return Ok(stats);
        }

        [HttpGet("challenge-count")]
        [ProducesResponseType(typeof(List<ChallengeCountDto>), 200)]
        public async Task<IActionResult> GetChallengeCount()
        {
            var stats = await _analyzingService.GetChallengeCountByDifficultyAsync();
            return Ok(stats);
        }

        [HttpGet("daily-player-ratio")]
        [ProducesResponseType(typeof(List<DailyPlayerRatioDto>), 200)]
        public async Task<IActionResult> GetDailyPlayerRatio()
        {
            var stats = await _analyzingService.GetDailyPlayerRatioAsync();
            return Ok(stats);
        }

        [HttpGet("top-10-users-score")]
        [ProducesResponseType(typeof(List<TopUserScoreDto>), 200)]
        public async Task<IActionResult> GetTop10UsersByScore()
        {
            var topUsers = await _analyzingService.GetTop10UsersByScoreAsync();
                
            if (topUsers == null || topUsers.Count == 0)
            {
                return Ok(new List<TopUserScoreDto>()); 
            }
                
            return Ok(topUsers);
        }
    }
}