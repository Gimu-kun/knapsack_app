// File: knapsack_app.Services/IGameService.cs (Interface)
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
            takenModel.TakenScore = 0;
            takenModel.TakenDuration = 0;
            takenModel.TakenAt = DateTime.UtcNow;
            _context.Taken.Add(takenModel);
            _context.SaveChanges();
            return Task.FromResult(newId);
        }

        public async Task<GameStatus> GetGameStatus(string takenId)
        {
            // Sử dụng EF Core LINQ để truy vấn TakenModel và JOIN ngầm với ChallengeModel (KsChallenge)
            // thông qua Navigation Property và sử dụng Projection (.Select) để chỉ lấy dữ liệu cần thiết.
            var sessionData = await _context.Taken
                // 1. Lọc theo Taken ID
                .Where(t => t.Id == takenId)
                // 2. Tạo một đối tượng vô danh (anonymous object) chỉ chứa TakenAt và MaxDuration
                .Select(t => new
                {
                    TakenAt = t.TakenAt,
                    // Giả định Navigation Property từ TakenModel đến ChallengeModel là 'Challenge'
                    // và thuộc tính thời gian tối đa là 'MaxDuration'
                    MaxDurationSeconds = t.Challenge.MaxDuration
                })
                // 3. Thực thi truy vấn và lấy kết quả đầu tiên (hoặc null)
                .FirstOrDefaultAsync();

            if (sessionData == null)
            {
                return null; // Không tìm thấy phiên chơi
            }

            DateTime takenAt = sessionData.TakenAt;
            int maxDurationSeconds = (int)sessionData.MaxDurationSeconds;

            // Tính toán thời gian đã trôi qua
            // Quan trọng: Phải dùng DateTime.UtcNow để so sánh với TakenAt (đã lưu là UTC)
            TimeSpan elapsedTime = DateTime.UtcNow.Subtract(takenAt);

            // Tính toán thời gian còn lại (làm tròn lên để đảm bảo đếm hết giây cuối cùng)
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
            // 1. Tìm bản ghi Taken (takenScore phải được load để cập nhật)
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

            // 2. Kiểm tra và giới hạn điểm tối thiểu là 0
            if (newScore < 0)
            {
                taken.TakenScore = 0; // Đặt lại điểm về 0
                isZeroScore = true;
            }
            else
            {
                taken.TakenScore = newScore;
            }

            // 3. Lưu thay đổi vào CSDL
            // EF Core sẽ tự động tạo lệnh UPDATE và thực thi
            await _context.SaveChangesAsync();

            // 4. Trả về kết quả
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
            // 1. Tìm bản ghi Taken (takenScore phải được load để cập nhật)
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

            // 4. Trả về kết quả
            return new AdjustScoreResponse
            {
                Success = true,
                Message = "Cập nhật kết quả thành công."
            };
        }
    }
}