using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly LeaderBoardService _leaderboardService;

    public LeaderboardController(LeaderBoardService leaderboardService)
    {
        _leaderboardService = leaderboardService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int playerCount,
        [FromQuery] int difficulty)
    {
        if (playerCount < 1 || playerCount > 4)
        {
            return BadRequest("Số lượng người chơi (playerCount) phải nằm trong khoảng từ 1 đến 4.");
        }

        if (difficulty < 1 || difficulty > 3)
        {
            return BadRequest("Độ khó (difficulty) phải nằm trong khoảng từ 1 đến 3.");
        }

        var results = await _leaderboardService.GetLeaderboard(playerCount, difficulty);

        return Ok(results);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var results = await _leaderboardService.GetPlayerOrTeamHistory(id);

        if (results == null || results.Count == 0)
        {
            return NotFound("Không tìm thấy dữ liệu xếp hạng.");
        }

        return Ok(results);
    }
}