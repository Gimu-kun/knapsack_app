using knapsack_app.Models;
using knapsack_app.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace knapsack_app.Services
{
    public class ChallengeService
    {
        private readonly AppDbContext _context;
        private readonly Random _rng = new Random();

        public ChallengeService(AppDbContext context)
        {
            _context = context;
        }

        private (int[][] DpBoard, List<ItemDto> ResultItems) SolveKnapsack(List<ItemDto> items, int maxCapacity)
        {
            int N = items.Count;
            int W = maxCapacity;
            int[,] dp = new int[N + 1, W + 1];

            for (int i = 0; i <= N; i++)
            {
                for (int w = 0; w <= W; w++)
                {
                    if (i == 0 || w == 0)
                    {
                        dp[i, w] = 0;
                    }
                    else if (items[i - 1].Weight <= w)
                    {
                        dp[i, w] = Math.Max(items[i - 1].Value + dp[i - 1, w - items[i - 1].Weight], dp[i - 1, w]);
                    }
                    else
                    {
                        dp[i, w] = dp[i - 1, w];
                    }
                }
            }

            int[][] jaggedDpBoard = new int[N + 1][];
            for (int i = 0; i <= N; i++)
            {
                jaggedDpBoard[i] = new int[W + 1];
                for (int w = 0; w <= W; w++)
                {
                    jaggedDpBoard[i][w] = dp[i, w];
                }
            }

            List<ItemDto> selectedItems = new List<ItemDto>();
            int res = dp[N, W];
            int currentW = W;
            for (int i = N; i > 0 && res > 0; i--)
            {
                if (res != dp[i - 1, currentW])
                {
                    selectedItems.Add(items[i - 1]);
                    res -= items[i - 1].Value;
                    currentW -= items[i - 1].Weight;
                }
            }

            return (jaggedDpBoard, selectedItems);
        }

        private List<MissCellDto> GenerateMissingCells(int[][] dpBoard, int missCount)
        {
            int N = dpBoard.Length;
            int W = dpBoard.Length > 0 ? dpBoard[0].Length : 0;

            List<(int x, int y, int value)> validCells = new List<(int x, int y, int value)>();

            for (int i = 1; i < N; i++)
            {
                if (dpBoard[i] == null) continue;
                for (int j = 1; j < W; j++)
                {
                    if (i == N - 1 && j == W) continue;
                    validCells.Add((i, j, dpBoard[i][j]));
                }
            }

            var uniqueValueCells = validCells
                .Where(c => c.value > 0)
                .GroupBy(c => c.value)
                .Select(g => g.First())
                .ToList();

            missCount = Math.Min(missCount, uniqueValueCells.Count);

            var n = uniqueValueCells.Count;
            while (n > 1)
            {
                n--;
                int k = _rng.Next(n + 1);
                var value = uniqueValueCells[k];
                uniqueValueCells[k] = uniqueValueCells[n];
                uniqueValueCells[n] = value;
            }

            return uniqueValueCells
                .Take(missCount)
                .Select(cell => new MissCellDto { X = cell.x, Y = cell.y })
                .ToList();
        }


        public async Task<(IEnumerable<ChallengeViewModel> Challenges, int TotalCount)> GetPaginatedChallengesAsync(int pageIndex, int pageSize, string searchTerm)
        {
            var query = _context.KsChallenge.AsNoTracking();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c =>
                    c.Id.Contains(searchTerm) ||
                    EF.Functions.Like(c.Difficulty.ToString(), $"%{searchTerm}%"));
            }

            var totalCount = await query.CountAsync();

            var challenges = await query
                .OrderBy(c => c.Difficulty)
                .ThenByDescending(c => c.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new ChallengeViewModel
                {
                    Id = c.Id,
                    Difficulty = c.Difficulty,
                    MaxCapacity = c.MaxCapacity ?? 0,
                    MaxDuration = c.MaxDuration ?? 0,
                    MissCount = c.MissCount ?? 0,
                    CreatedAt = c.CreatedAt,
                    CreatedBy = c.CreatedBy ?? "N/A"
                })
                .ToListAsync();

            return (challenges, totalCount);
        }

        public async Task<ChallengeCreateEditModel?> GetChallengeByIdAsync(string id)
        {
            var challenge = await _context.KsChallenge
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (challenge == null) return null;

            return new ChallengeCreateEditModel
            {
                Id = challenge.Id,
                Difficulty = challenge.Difficulty,
                MaxCapacity = challenge.MaxCapacity ?? 0,
                MaxDuration = challenge.MaxDuration ?? 0,
                MissCount = challenge.MissCount ?? 0,
                QuesDataJson = challenge.QuesData,
                DpBoardJson = challenge.DpBoard,
                ResultItemsJson = challenge.ResultItems ?? "[]",
                MissArrayJson = challenge.MissArray ?? "[]"
            };
        }

        public async Task<string> CreateChallengeAsync(ChallengeCreateEditModel model, string operatorId)
        {
            List<ItemDto> items;
            try
            {
                items = JsonSerializer.Deserialize<List<ItemDto>>(model.QuesDataJson) ?? throw new Exception("Dữ liệu vật phẩm rỗng hoặc không hợp lệ.");

                foreach (var item in items.Where(item => string.IsNullOrEmpty(item.Id)))
                {
                    item.Id = Guid.NewGuid().ToString();
                }

                model.QuesDataJson = JsonSerializer.Serialize(items);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Lỗi deserialize hoặc gán ID cho vật phẩm: {ex.Message}");
            }

            var (dpBoardArray, selectedItems) = SolveKnapsack(items, model.MaxCapacity);

            var missingCells = GenerateMissingCells(dpBoardArray, model.MissCount);

            model.DpBoardJson = JsonSerializer.Serialize(dpBoardArray);
            model.ResultItemsJson = JsonSerializer.Serialize(selectedItems);
            model.MissArrayJson = JsonSerializer.Serialize(missingCells);

            var newChallenge = new KsChallengeModel
            {
                Difficulty = model.Difficulty,
                MaxCapacity = model.MaxCapacity,
                MaxDuration = model.MaxDuration,
                MissCount = model.MissCount,
                QuesData = model.QuesDataJson,
                DpBoard = model.DpBoardJson,
                ResultItems = model.ResultItemsJson,
                MissArray = model.MissArrayJson,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedBy = operatorId,
                UpdatedBy = operatorId
            };

            _context.KsChallenge.Add(newChallenge);
            await _context.SaveChangesAsync();
            return newChallenge.Id;
        }

        public async Task UpdateChallengeAsync(ChallengeCreateEditModel model, string operatorId)
        {
            var existingChallenge = await _context.KsChallenge.FindAsync(model.Id);

            if (existingChallenge == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy đề bài với ID '{model.Id}'.");
            }

            existingChallenge.Difficulty = model.Difficulty;
            existingChallenge.MaxCapacity = model.MaxCapacity;
            existingChallenge.MaxDuration = model.MaxDuration;
            existingChallenge.MissCount = model.MissCount;

            existingChallenge.QuesData = model.QuesDataJson;
            existingChallenge.DpBoard = model.DpBoardJson;
            existingChallenge.ResultItems = model.ResultItemsJson;
            existingChallenge.MissArray = model.MissArrayJson;

            existingChallenge.UpdatedAt = DateTime.Now;
            existingChallenge.UpdatedBy = operatorId;

            _context.KsChallenge.Update(existingChallenge);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteChallengeAsync(string id)
        {
            var challenge = await _context.KsChallenge.FindAsync(id);
            if (challenge != null)
            {
                _context.KsChallenge.Remove(challenge);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.KsChallenge.AnyAsync(c => c.Id == id);
        }

        public async Task<ChallengeCreateEditModel?> GetRandomOrSpecificChallengeAsync(
            DifficultyEnum difficulty, string? challengeId)
        {
            IQueryable<KsChallengeModel> query = _context.KsChallenge
                .AsNoTracking()
                .Where(c => c.Difficulty == difficulty);

            KsChallengeModel? challenge;

            if (!string.IsNullOrEmpty(challengeId))
            {
                challenge = await query.FirstOrDefaultAsync(c => c.Id == challengeId);
            }
            else
            {
                var totalCount = await query.CountAsync();

                if (totalCount == 0)
                {
                    return null;
                }
                var skipCount = _rng.Next(totalCount);

                challenge = await query
                    .OrderBy(c => c.Id)
                    .Skip(skipCount)
                    .Take(1)
                    .FirstOrDefaultAsync();
            }

            if (challenge == null) return null;

            return new ChallengeCreateEditModel
            {
                Id = challenge.Id,
                Difficulty = challenge.Difficulty,
                MaxCapacity = challenge.MaxCapacity ?? 0,
                MaxDuration = challenge.MaxDuration ?? 0,
                MissCount = challenge.MissCount ?? 0,
                QuesDataJson = challenge.QuesData,
                DpBoardJson = challenge.DpBoard,
                ResultItemsJson = challenge.ResultItems ?? "[]",
                MissArrayJson = challenge.MissArray ?? "[]"
            };
        }
    }
}