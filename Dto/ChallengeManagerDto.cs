using System.ComponentModel.DataAnnotations;

namespace knapsack_app.ViewModels
{
    // DTO để hiển thị danh sách đề bài trên bảng
    public class ChallengeViewModel
    {
        public string Id { get; set; } = string.Empty;
        public DifficultyEnum Difficulty { get; set; }
        public int MaxCapacity { get; set; }
        public int MaxDuration { get; set; }
        public int MissCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = "ADMIN";
    }

    public class ChallengeCreateEditModel
    {
        public string? Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Độ khó là bắt buộc.")]
        public DifficultyEnum Difficulty { get; set; }

        [Required(ErrorMessage = "Dữ liệu câu hỏi (ques_data) là bắt buộc.")]
        public string QuesDataJson { get; set; } = "[]";

        public string? DpBoardJson { get; set; } = "[]";

        public string? ResultItemsJson { get; set; } = "[]"; 

        [Required(ErrorMessage = "Max Capacity là bắt buộc.")]
        [Range(1, 1000, ErrorMessage = "Capacity phải từ 1 đến 1000.")]
        public int MaxCapacity { get; set; }

        [Required(ErrorMessage = "Max Duration là bắt buộc.")]
        [Range(1, 300, ErrorMessage = "Duration phải từ 1 đến 300 giây.")]
        public int MaxDuration { get; set; }

        public int MissCount { get; set; } = 0;
        public string? MissArrayJson { get; set; } = "[]";
    }
}
