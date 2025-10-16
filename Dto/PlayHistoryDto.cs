namespace knapsack_app.Models.Models
{
    public class PlayHistoryApiViewModel
    {
        public string ChallengeId { get; set; }
        public int TakenScore { get; set; }
        public int TakenDuration { get; set; }
        public DateTime TakenAt { get; set; }
        public bool IsMultiplay { get; set; }
        public DifficultyEnum ChallengeDifficulty { get; set; } 
    }
}