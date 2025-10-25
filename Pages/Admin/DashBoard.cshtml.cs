
using knapsack_app.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace knapsack_app.Pages.Dashboard
{
    public class DashBoardModel : PageModel
    {
        private readonly AnalyzingService _analyzingService;

        public List<DailyChallengeStatDto> DailyStats { get; set; } = new List<DailyChallengeStatDto>();

        public string EasyChartData { get; set; } = "[]";
        public string MediumChartData { get; set; } = "[]";
        public string HardChartData { get; set; } = "[]";
        public string ChartLabels { get; set; } = "[]";

        public int TotalTakenCount { get; set; }
        public decimal EasyPercentage { get; set; }
        public decimal MediumPercentage { get; set; }
        public decimal HardPercentage { get; set; }

        public string EasyChangePercent { get; set; } = "N/A";
        public string MediumChangePercent { get; set; } = "N/A";
        public string HardChangePercent { get; set; } = "N/A";

        public string ChallengeCountLabels { get; set; } = "[]";
        public string ChallengeCountData { get; set; } = "[]";
        
        public string DailyRatioDates { get; set; } = "[]";
        public string SoloRatioData { get; set; } = "[]";
        public string DuoRatioData { get; set; } = "[]";

        public string TrioRatioData { get; set; } = "[]";

        public string QuarRatioData { get; set; } = "[]";

        public List<TopUserScoreDto> TopUsers { get; set; } = new List<TopUserScoreDto>();

        public DashBoardModel(AnalyzingService analyzingService)
        {
            _analyzingService = analyzingService;
        }

        public async Task OnGetAsync()
        {
            DailyStats = await _analyzingService.GetDailyTakenStatsAsync();
            ProcessChartData(DailyStats);

            var challengeCounts = await _analyzingService.GetChallengeCountByDifficultyAsync();
            
            ChallengeCountLabels = System.Text.Json.JsonSerializer.Serialize(
                challengeCounts.Select(c => c.Difficulty.ToUpper()).ToList());
            ChallengeCountData = System.Text.Json.JsonSerializer.Serialize(
                challengeCounts.Select(c => c.TotalChallenges).ToList());

            var dailyRatios = await _analyzingService.GetDailyPlayerRatioAsync();

            if (dailyRatios.Any())
            {
                DailyRatioDates = System.Text.Json.JsonSerializer.Serialize(
                    dailyRatios.Select(d => d.TakenDate.ToString("MM/dd")).ToList());

                SoloRatioData = System.Text.Json.JsonSerializer.Serialize(
                    dailyRatios.Select(d => d.Ratios.FirstOrDefault(r => r.PlayerCount == 1)?.RatioPercentage ?? 0).ToList());

                DuoRatioData = System.Text.Json.JsonSerializer.Serialize(
                    dailyRatios.Select(d => d.Ratios.FirstOrDefault(r => r.PlayerCount == 2)?.RatioPercentage ?? 0).ToList());

                TrioRatioData = System.Text.Json.JsonSerializer.Serialize(
                   dailyRatios.Select(d => d.Ratios.FirstOrDefault(r => r.PlayerCount == 3)?.RatioPercentage ?? 0).ToList());

                QuarRatioData = System.Text.Json.JsonSerializer.Serialize(
                   dailyRatios.Select(d => d.Ratios.FirstOrDefault(r => r.PlayerCount == 4)?.RatioPercentage ?? 0).ToList());

            }
            
            TopUsers = await _analyzingService.GetTop10UsersByScoreAsync();
        }

        private void ProcessChartData(List<DailyChallengeStatDto> stats)
        {
            if (!stats.Any()) return;

            var allDates = stats
                .Select(s => s.TakenDate)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            ChartLabels = System.Text.Json.JsonSerializer.Serialize(allDates.Select(d => d.ToString("MM/dd")).ToList());

            var dataLookup = stats.ToDictionary(s => (s.TakenDate, s.Difficulty), s => s.TakenCount);
            
            var easyData = allDates.Select(date => dataLookup.GetValueOrDefault((date, "easy"), 0)).ToList();
            var mediumData = allDates.Select(date => dataLookup.GetValueOrDefault((date, "medium"), 0)).ToList();
            var hardData = allDates.Select(date => dataLookup.GetValueOrDefault((date, "hard"), 0)).ToList();

            EasyChartData = System.Text.Json.JsonSerializer.Serialize(easyData);
            MediumChartData = System.Text.Json.JsonSerializer.Serialize(mediumData);
            HardChartData = System.Text.Json.JsonSerializer.Serialize(hardData);
            
            TotalTakenCount = stats.Sum(s => s.TakenCount);
        
            if (TotalTakenCount > 0)
            {
                var easyCount = easyData.Sum();
                var mediumCount = mediumData.Sum();
                var hardCount = hardData.Sum();

                EasyPercentage = (decimal)easyCount / TotalTakenCount * 100;
                MediumPercentage = (decimal)mediumCount / TotalTakenCount * 100;
                HardPercentage = (decimal)hardCount / TotalTakenCount * 100;
            }
        }
    }
}