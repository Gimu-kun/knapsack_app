// File: knapsack_app.Services/IGameService.cs (Interface)
using knapsack_app.Models;
using knapsack_app.ViewModels;
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
    }
}