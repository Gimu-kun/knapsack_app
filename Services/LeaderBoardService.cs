using knapsack_app.Models;
using knapsack_app.ViewModels;
using Microsoft.EntityFrameworkCore;

public class LeaderBoardService
{
    private readonly AppDbContext _context;

    public LeaderBoardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<LeaderboardEntryDto>> GetLeaderboard(
            int playerCount,
            int difficulty)
    {
        var difficultyEnum = (DifficultyEnum)difficulty;

        var joinedQuery = _context.Taken.AsQueryable()
            .Where(t => t.PlayerCount == playerCount)
            .Join(
                _context.KsChallenge,
                taken => taken.ChallengeId,
                challenge => challenge.Id,
                (taken, challenge) => new { Taken = taken, Challenge = challenge }
            )
            .Where(joined => joined.Challenge.Difficulty == difficultyEnum);
        var groupedQuery = joinedQuery
            .GroupBy(joined => playerCount == 1 ? joined.Taken.UserId : joined.Taken.TeamId);

        var leaderboard = await groupedQuery
            .Select(g => new LeaderboardEntryDto
            {
                RankKey = g.Key,
                PlayerCount = playerCount,
                Avatar = _context.User.Where(u => u.Id == g.Key).Select(u => u.Avatar).FirstOrDefault(),
                Name = playerCount == 1
                    ? _context.User
                        .Where(u => u.Id == g.Key)
                        .Select(u => u.Account)
                        .FirstOrDefault() ?? "Người chơi ẩn danh"
                    : g.First().Taken.TeamName ?? "Đội ẩn danh",

                Score = g.Max(t => t.Taken.TakenScore),
                Duration = g.Min(t => t.Taken.TakenDuration),
                LastPlayedAt = g.Max(t => t.Taken.TakenAt),

                TeamMembers = playerCount > 1
                    ? g
                        .Select(t => t.Taken.UserId)
                        .Distinct()
                        .Select(userId => new TeamMemberDto
                        {
                            UserId = userId,
                            Avatar = _context.User
                                .Where(u => u.Id == userId)
                                .Select(u => u.Avatar)
                                .FirstOrDefault(),
                            Username = _context.User
                                .Where(u => u.Id == userId)
                                .Select(u => u.Account)
                                .FirstOrDefault() ?? "Ẩn danh"
                        })
                        .ToList()
                    : new List<TeamMemberDto>()
            })
            .OrderByDescending(dto => dto.Score)
            .ThenBy(dto => dto.Duration)
            .Take(50)
            .ToListAsync();
        for (int i = 0; i < leaderboard.Count; i++)
        {
            leaderboard[i].Rank = i + 1;
        }

        return leaderboard;
    }

    public async Task<List<TakenModel>> GetPlayerOrTeamHistory(string id)
    {
        // Giả định ID đầu vào là team_id HOẶC user_id
        var history = await _context.Taken
            .Where(t => t.TeamId == id || t.UserId == id)
            .OrderByDescending(t => t.TakenAt)
            .ToListAsync();

        return history;
    }
}