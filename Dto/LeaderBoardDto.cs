namespace knapsack_app.ViewModels
{
    public class LeaderboardEntryDto
    {

        public string RankKey { get; set; } = string.Empty;

        public string Name { get; set; } = "Người chơi/Đội ẩn danh";

        public int Score { get; set; }

        public int Duration { get; set; } 

        public int PlayerCount { get; set; }

        public DateTime LastPlayedAt { get; set; }
        
        public List<TeamMemberDto> TeamMembers { get; set; } = new List<TeamMemberDto>();

        public int Rank { get; set; }
    }

    public class TeamMemberDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
    }
}