public class DailyChallengeStatDto
{
    public DateOnly TakenDate { get; set; }

    public string Difficulty { get; set; }

    public int TakenCount { get; set; }
}

public class ChallengeCountDto
{
    public string Difficulty { get; set; }
    public int TotalChallenges { get; set; }
}

public class DailyPlayerRatioDto
{
    public DateOnly TakenDate { get; set; }
    public int TotalSessions { get; set; }
    public List<PlayerCountRatio> Ratios { get; set; }
}

public class PlayerCountRatio
{
    public int PlayerCount { get; set; }
    public int SessionCount { get; set; }
    public decimal RatioPercentage { get; set; }
}

public class TopUserScoreDto
{
    public string UserId { get; set; }
    public string Account { get; set; }
    public long AccumulatedScore { get; set; }
}

public class UserProgressDto
{
    public string UserId { get; set; }
    public string Account { get; set; }
    public DateTime CreatedAt { get; set; }
    public LoginFrequencyDto LoginStatus { get; set; }
    public List<DailyActivityDto> DailyActivities { get; set; }
}

public class LoginFrequencyDto
{
    public DateTime? LastLogin { get; set; }
    public int DaysSinceRegistration { get; set; }
}

public class DailyActivityDto
{
    public DateOnly TakenDate { get; set; }
    public long TotalScoreAchieved { get; set; }
    public int TotalChallengesPlayed { get; set; }
    public Dictionary<string, int> ChallengesByDifficulty { get; set; } 
}