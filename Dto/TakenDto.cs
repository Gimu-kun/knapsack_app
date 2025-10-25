namespace knapsack_app.ViewModels
{
    public class StartGameRequest
    {
        public string ChallengeId { get; set; }
        public string UserId { get; set; }
        public string? TeamName { get; set; } 
        public string? TeamId { get; set; }
        public int PlayerCount { get; set; } = 1;
    }

    public class GameSessionStarted
    {
        public string TakenId { get; set; }
        public int MaxDurationSeconds { get; set; }
        public DateTime StartTimeUtc { get; set; }
        public DateTime DeadlineUtc { get; set; }
    }

    public class GameStatus
    {
        public int TimeRemainingSeconds { get; set; }
        public bool IsTimeUp { get; set; }
    }

    public class AdjustScoreRequest
    {
        public string TakenId { get; set; }
        public int ScoreChange { get; set; }
    }

    public class AdjustScoreResponse
    {
        public bool Success { get; set; }
        public int NewScore { get; set; }
        public bool IsZeroScore { get; set; }
        public string Message { get; set; }
    }
}