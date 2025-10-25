using knapsack_app.Models;
using knapsack_app.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
namespace knapsack_app.Services
{
    public class AnalyzingService
    {
        private readonly AppDbContext _context;

        public AnalyzingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<DailyChallengeStatDto>> GetDailyTakenStatsAsync()
        {
            var stats = await _context.Taken
                .Join(
                    _context.KsChallenge,
                    taken => taken.ChallengeId,
                    challenge => challenge.Id,
                    (taken, challenge) => new { taken, challenge }
                )
                .GroupBy(
                    x => new
                    {
                        TakenDate = DateOnly.FromDateTime(x.taken.TakenAt),
                        x.challenge.Difficulty
                    }
                )
                .Select(g => new DailyChallengeStatDto
                {
                    TakenDate = g.Key.TakenDate,
                    Difficulty = g.Key.Difficulty.ToString(),
                    TakenCount = g.Count()
                })
                .OrderBy(s => s.TakenDate)
                .ThenBy(s => s.Difficulty == "easy" ? 1 : s.Difficulty == "medium" ? 2 : 3)
                .ToListAsync();

            return stats;
        }

        public async Task<List<ChallengeCountDto>> GetChallengeCountByDifficultyAsync()
        {
            return await _context.KsChallenge
                .GroupBy(c => c.Difficulty)
                .Select(g => new ChallengeCountDto
                {
                    Difficulty = g.Key.ToString(),
                    TotalChallenges = g.Count()
                })
                .OrderBy(d => d.Difficulty)
                .ToListAsync();
        }

        public async Task<List<DailyPlayerRatioDto>> GetDailyPlayerRatioAsync()
        {
            var uniqueSessions = await _context.Taken
                .GroupBy(t => new
                {
                    Date = DateOnly.FromDateTime(t.TakenAt),
                    t.ChallengeId,
                    TeamIdentifier = t.TeamId ?? t.Id 
                })
                .Select(g => new
                {
                    TakenDate = g.Key.Date,
                    PlayerCount = g.First().PlayerCount 
                })
                .ToListAsync();

            var dailyStats = uniqueSessions
                .GroupBy(s => s.TakenDate)
                .Select(g => new DailyPlayerRatioDto
                {
                    TakenDate = g.Key,
                    TotalSessions = g.Count(),
                    Ratios = g.GroupBy(s => s.PlayerCount)
                        .Select(pg => new PlayerCountRatio
                        {
                            PlayerCount = pg.Key,
                            SessionCount = pg.Count(),
                            RatioPercentage = (decimal)pg.Count() / g.Count() * 100
                        })
                        .OrderBy(r => r.PlayerCount)
                        .ToList()
                })
                .OrderBy(d => d.TakenDate)
                .ToList();

            return dailyStats;
        }
        
        public async Task<List<TopUserScoreDto>> GetTop10UsersByScoreAsync()
        {
            var rankedScores = await _context.Taken
                .GroupBy(t => t.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    AccumulatedScore = g.Sum(t => t.TakenScore)
                })
                .OrderByDescending(x => x.AccumulatedScore)
                .Take(10)
                .ToListAsync();

            var userIds = rankedScores.Select(x => x.UserId).ToList();
            
            var users = await _context.User
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Account);

            var topUsers = rankedScores.Select(rs => new TopUserScoreDto
            {
                UserId = rs.UserId,
                Account = users.GetValueOrDefault(rs.UserId, "Unknown User"),
                AccumulatedScore = rs.AccumulatedScore
            }).ToList();

            return topUsers;
        }
    }
}