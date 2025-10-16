using knapsack_app.Models.Models;
using Microsoft.EntityFrameworkCore;

namespace knapsack_app.Services
{
    public class HistoryService
    {
        private readonly AppDbContext _context;

        public HistoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PlayHistoryApiViewModel>> GetUserPlayHistoryAsync(string userId)
        {
            var history = await _context.Taken 
                .AsNoTracking() 
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.TakenAt)
                .Select(t => new PlayHistoryApiViewModel
                {
                    ChallengeId = t.ChallengeId,
                    TakenScore = t.TakenScore,
                    TakenDuration = t.TakenDuration,
                    TakenAt = t.TakenAt,
                    IsMultiplay = t.IsMultiplay,
                    ChallengeDifficulty = t.Challenge.Difficulty,
                })
                .ToListAsync();
            
            return history;
        }
    }
}