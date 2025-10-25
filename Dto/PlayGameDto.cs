namespace knapsack_app.ViewModels
{
    public class DpCell
    {
        public int I { get; set; }
        public int W { get; set; }
        public int CorrectValue { get; set; }
        public bool IsInput { get; set; }

        public string Id => $"dp-cell-{I}-{W}";
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