namespace knapsack_app.ViewModels
{
    public class PlayHistoryApiViewModel
    {
        public string ChallengeId { get; set; }
        public int TakenScore { get; set; }
        public int TakenDuration { get; set; }
        public DateTime TakenAt { get; set; }
        public string TeamName { get; set; }
        public string TeamId { get; set; }
        public DifficultyEnum ChallengeDifficulty { get; set; } 
    }
}