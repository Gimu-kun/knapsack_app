namespace knapsack_app.ViewModels
{
    public class DpCell
    {
        public int I { get; set; } // Hàng (vật phẩm)
        public int W { get; set; } // Cột (sức chứa)
        public int CorrectValue { get; set; } // Giá trị đúng (lấy từ DPBoard)
        public bool IsInput { get; set; } // Có phải là ô người dùng cần nhập không

        public string Id => $"dp-cell-{I}-{W}"; // ID HTML cho ô
    }

    public class GameData
    {
        public string Id { get; set; } = string.Empty;
        public int Difficulty { get; set; }
        public int MaxCapacity { get; set; }
        public int MaxDuration { get; set; }
        public int MissCount { get; set; }
        
        public string QuesDataJson { get; set; } = string.Empty;
        public string DpBoardJson { get; set; } = string.Empty;
        public string ResultItemsJson { get; set; } = string.Empty;
        public string MissArrayJson { get; set; } = string.Empty;
    }
}