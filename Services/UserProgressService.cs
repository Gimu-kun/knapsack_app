using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class UserProgressService
{
    private readonly AppDbContext _context;

    public UserProgressService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserProgressDto> GetUserProgressAsync(string userId)
    {
        var user = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return null;

        var progress = new UserProgressDto
        {
            CreatedAt = user.CreatedAt,
            UserId = user.Id,
            Account = user.Account,
            LoginStatus = new LoginFrequencyDto
            {
                LastLogin = user.LastLogin,
                DaysSinceRegistration = (int)(DateTime.Now.Date - user.CreatedAt.Date).TotalDays
            }
        };

        var rawTakenData = await _context.Taken
            .Where(t => t.UserId == userId)
            .Join(
                _context.KsChallenge,
                t => t.ChallengeId,
                c => c.Id,
                (t, c) => new 
                {
                    Date = DateOnly.FromDateTime(t.TakenAt),
                    t.TakenScore,
                    c.Difficulty 
                }
            )
            .ToListAsync();
            
        progress.DailyActivities = rawTakenData
            .GroupBy(x => x.Date)
            .Select(g => new DailyActivityDto
            {
                TakenDate = g.Key,
                TotalScoreAchieved = g.Sum(x => x.TakenScore),
                TotalChallengesPlayed = g.Count(), 
                ChallengesByDifficulty = g
                    .GroupBy(x => x.Difficulty)
                    .ToDictionary(
                        dg => dg.Key.ToString(),
                        dg => dg.Count()
                    )
            })
            .OrderBy(s => s.TakenDate)
            .ToList();

        return progress;
    }
}