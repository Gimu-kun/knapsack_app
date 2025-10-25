using knapsack_app.Models;
using knapsack_app.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
namespace knapsack_app.Services
{
    public class GameService
    {
        private readonly AppDbContext _context;

        public GameService(AppDbContext context)
        {
            _context = context;
        }

        public Task<string> CreateNewTakenSession(StartGameRequest request, ChallengeCreateEditModel challengeInfo, DateTime startTime)
        {
            string newId = "TK-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            TakenModel takenModel = new TakenModel();
            takenModel.Id = newId;
            takenModel.UserId = request.UserId;
            takenModel.ChallengeId = request.ChallengeId;
            takenModel.TeamId = request.TeamId;
            takenModel.TeamName = request.TeamName;
            takenModel.PlayerCount = request.PlayerCount;
            takenModel.TakenScore = 500;
            takenModel.TakenDuration = 0;
            takenModel.TakenAt = DateTime.UtcNow;
            _context.Taken.Add(takenModel);
            _context.SaveChanges();
            return Task.FromResult(newId);
        }

        public async Task<GameStatus> GetGameStatus(string takenId)
        {
            var sessionData = await _context.Taken
                .Where(t => t.Id == takenId)
                .Select(t => new
                {
                    TakenAt = t.TakenAt,
                    MaxDurationSeconds = t.Challenge.MaxDuration
                })
                .FirstOrDefaultAsync();

            if (sessionData == null)
            {
                return null;
            }

            DateTime takenAt = sessionData.TakenAt;
            int maxDurationSeconds = (int)sessionData.MaxDurationSeconds;

            TimeSpan elapsedTime = DateTime.UtcNow.Subtract(takenAt);

            int timeRemainingSeconds = maxDurationSeconds - (int)Math.Ceiling(elapsedTime.TotalSeconds);

            bool isTimeUp = timeRemainingSeconds <= 0;

            return new GameStatus
            {
                TimeRemainingSeconds = Math.Max(0, timeRemainingSeconds),
                IsTimeUp = isTimeUp
            };
        }

        public async Task<AdjustScoreResponse> AdjustTakenScore(string takenId, int scoreChange)
        {
            var taken = await _context.Taken
                .FirstOrDefaultAsync(t => t.Id == takenId);

            Console.WriteLine("Tìm được taken ? " + $"{taken != null}");

            if (taken == null)
            {
                return new AdjustScoreResponse
                {
                    Success = false,
                    Message = $"Phiên chơi (Taken ID: {takenId}) không tồn tại."
                };
            }

            int currentScore = taken.TakenScore;
            int newScore = currentScore + scoreChange;
            bool isZeroScore = false;

            if (newScore < 0)
            {
                taken.TakenScore = 0; 
                isZeroScore = true;
            }
            else
            {
                taken.TakenScore = newScore;
            }

            await _context.SaveChangesAsync();

            return new AdjustScoreResponse
            {
                Success = true,
                NewScore = taken.TakenScore,
                IsZeroScore = isZeroScore,
                Message = isZeroScore ? "Điểm đã bị trừ về 0." : "Điểm đã được điều chỉnh thành công."
            };
        }
        
         public async Task<AdjustScoreResponse> EndGame(string TakenId, int takenTimeSeconds)
        {
            var taken = await _context.Taken
                .FirstOrDefaultAsync(t => t.Id == TakenId);

            if (taken == null)
            {
                return new AdjustScoreResponse
                {
                    Success = false,
                    Message = $"Phiên chơi (Taken ID: {TakenId}) không tồn tại."
                };
            };

            taken.TakenDuration = takenTimeSeconds;
            _context.Taken.Update(taken);
            await _context.SaveChangesAsync();

            return new AdjustScoreResponse
            {
                Success = true,
                Message = "Cập nhật kết quả thành công."
            };
        }
    }
}