using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json; 
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using knapsack_app.ViewModels;

namespace knapsack_app.Pages
{
    public class LeaderboardModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        private const string ApiBaseUrl = "http://localhost:5238/api";

        public List<LeaderboardEntryDto> LeaderboardData { get; set; } = new List<LeaderboardEntryDto>();

        public int SelectedModeInt { get; set; } 
        public int SelectedDifficultyInt { get; set; }
        
        public string SelectedDifficultyString { get; set; } = "EASY";

        public LeaderboardModel(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
        }

        public async Task OnGetAsync(int? players, int? difficulty)
        {
            int mode = players.GetValueOrDefault(1);
            SelectedModeInt = Math.Clamp(mode, 1, 4); 
            
            int diff = difficulty.GetValueOrDefault(1);
            SelectedDifficultyInt = Math.Clamp(diff, 1, 3);

            SelectedDifficultyString = ((DifficultyEnum)SelectedDifficultyInt).ToString().ToUpper();
            
            string apiUrl = $"{ApiBaseUrl}/Leaderboard?playerCount={SelectedModeInt}&difficulty={SelectedDifficultyInt}";
            try
            {
                var response = await _httpClient.GetAsync(apiUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    
                    LeaderboardData = JsonSerializer.Deserialize<List<LeaderboardEntryDto>>(jsonString, options) ?? new List<LeaderboardEntryDto>();

                    for (int i = 0; i < LeaderboardData.Count; i++)
                    {
                        LeaderboardData[i].Rank = i + 1;
                    }
                }
                else
                {
                    LeaderboardData = new List<LeaderboardEntryDto>(); 
                }
            }
            catch (Exception ex)
            {
                LeaderboardData = new List<LeaderboardEntryDto>(); 
            }
        }
    }
}