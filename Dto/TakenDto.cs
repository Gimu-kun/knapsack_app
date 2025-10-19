namespace knapsack_app.ViewModels
{
    // DTO để nhận thông tin từ Front-end khi người dùng nhấn Start
    public class StartGameRequest
    {
        public string ChallengeId { get; set; }
        public string UserId { get; set; }
        // Có thể thêm TeamName/TeamId nếu áp dụng Multiplayer
        public string? TeamName { get; set; } 
        public string? TeamId { get; set; }
        public int PlayerCount { get; set; } = 1; // Giả định là chơi solo
    }

    // DTO trả về cho Front-end sau khi Start Game thành công
    public class GameSessionStarted
    {
        public string TakenId { get; set; } // ID phiên chơi đã được lưu
        public int MaxDurationSeconds { get; set; } // Thời gian tối đa cho thử thách
        public DateTime StartTimeUtc { get; set; } // Thời điểm phiên chơi bắt đầu (Server Time - Rất quan trọng!)
        public DateTime DeadlineUtc { get; set; } // Thời điểm hết hạn (StartTime + MaxDuration)
    }
}