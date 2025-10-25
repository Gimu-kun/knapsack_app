using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System; 
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using knapsack_app.ViewModels;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using knapsack_app.Models;

namespace knapsack_app.Pages.Cli
{

    public class PlayGameModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly JwtService _jwtService;
        private readonly UserService _userService;
        private const string ApiBaseUrl = "http://localhost:5238/api/Challenge/"; 

        public PlayGameModel(HttpClient httpClient, JwtService jwtService, UserService userService)
        {
            _httpClient = httpClient;
            _jwtService = jwtService;
            _userService = userService;
        }

        [BindProperty(SupportsGet = true)]
        public string Challenge_id { get; set; } = string.Empty;
        [BindProperty(SupportsGet = true)]
        public string? RoomId { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? TeamId { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? TeamName { get; set; }
        [BindProperty(SupportsGet = true)]
        public int Players { get; set; }
        public GameSessionStarted SessionData { get; set; } = new GameSessionStarted();
        public GameData InputGameData { get; set; } = new GameData();
        public List<KnapsackItem> Items { get; set; } = new List<KnapsackItem>();
        public int[,] DPBoard { get; set; } = new int[1, 1]; 
        
        public List<KnapsackItem> ResultItems { get; set; } = new List<KnapsackItem>();
        public List<MissCellDto> MissArray { get; set; } = new List<MissCellDto>(); 

        public bool GameStarted { get; set; } = false; 
        
        public int dpRows => DPBoard.GetLength(0);
        public int dpCols => DPBoard.GetLength(1);
        public int capacity => InputGameData.MaxCapacity;
        public string? userId;
        public UserModel CurrentUser { get; set; } = null;
        
        public int? InteractiveCells 
        {
            get
            {
                if (Items == null || InputGameData == null || InputGameData.MaxCapacity <= 0)
                {
                    return null;
                }
                
                return Items.Count * InputGameData.MaxCapacity;
            }
        }

        public int TotalInputCells => dpRows * dpCols; 
        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.Request.Cookies.TryGetValue("UserToken", out var token) || string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Cli/Login");
            }

            var principal = _jwtService.ValidateToken(token);
            if (principal == null)
            {
                return RedirectToPage("/Cli/Login");
            }

            userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Cli/Login");
            }

            Console.WriteLine("Đăng nhập thành công với userId: " + userId);
            CurrentUser = await _userService.GetUserById(userId);
            if (string.IsNullOrEmpty(Challenge_id))
            {
                return RedirectToPage("/Error"); 
            }

            try
            {
                string apiUrl = $"{ApiBaseUrl}{Challenge_id}";
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string jsonString = await response.Content.ReadAsStringAsync();
                    
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    InputGameData = JsonSerializer.Deserialize<GameData>(jsonString, options) ?? new GameData();

                    ParseNestedJson();
                    AssignItemNames();
                    SessionData.MaxDurationSeconds = InputGameData.MaxDuration;
                    return Page();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, $"Lỗi khi tải dữ liệu thử thách. Mã lỗi: {(int)response.StatusCode} - {response.ReasonPhrase}");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi hệ thống: Không thể kết nối hoặc xử lý dữ liệu. Chi tiết: {ex.Message}");
                return Page();
            }
        }

        private void ParseNestedJson()
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            
            if (!string.IsNullOrEmpty(InputGameData.QuesDataJson))
            {
                Items = JsonSerializer.Deserialize<List<KnapsackItem>>(InputGameData.QuesDataJson, options) ?? new List<KnapsackItem>();
            }

            if (!string.IsNullOrEmpty(InputGameData.DpBoardJson))
            {
                var tempList = JsonSerializer.Deserialize<List<List<int>>>(InputGameData.DpBoardJson, options);
                
                if (tempList != null && tempList.Count > 0 && tempList.First().Count > 0)
                {
                    int rows = tempList.Count;
                    int cols = tempList.First().Count;
                    DPBoard = new int[rows, cols];

                    for (int i = 0; i < rows; i++)
                    {
                        if (tempList[i] != null && tempList[i].Count == cols) 
                        {
                            for (int j = 0; j < cols; j++)
                            {
                                DPBoard[i, j] = tempList[i][j];
                            }
                        }
                    }
                }
                else
                {
                    DPBoard = new int[1, 1]; 
                }
            }
            
            if (!string.IsNullOrEmpty(InputGameData.ResultItemsJson))
            {
                ResultItems = JsonSerializer.Deserialize<List<KnapsackItem>>(InputGameData.ResultItemsJson, options) ?? new List<KnapsackItem>();
            }
            
            if (!string.IsNullOrEmpty(InputGameData.MissArrayJson))
            {
                MissArray = JsonSerializer.Deserialize<List<MissCellDto>>(InputGameData.MissArrayJson, options) ?? new List<MissCellDto>();
            }
        }

        private void AssignItemNames()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].Name = $"Item {((char)('A' + i)).ToString()}";
                Items[i].Id = (i + 1).ToString();
            }

            foreach (var resultItem in ResultItems)
            {
                var matchingItem = Items.FirstOrDefault(i => i.Id == resultItem.Id);
                if (matchingItem != null)
                {
                    resultItem.Name = matchingItem.Name;
                }
                else
                {
                    var originalItem = Items.FirstOrDefault(i =>
                        i.Value == resultItem.Value &&
                        i.Weight == resultItem.Weight);

                    if (originalItem != null)
                    {
                        resultItem.Name = originalItem.Name;
                    }
                }
            }
        }

        public async Task<IActionResult> OnPostStartGameAsync()
        {
            if (!HttpContext.Request.Cookies.TryGetValue("UserToken", out var token) || string.IsNullOrEmpty(token))
            {
                return new JsonResult(new { success = false, message = "Người dùng chưa đăng nhập." }) { StatusCode = 401 };
            }

            var principal = _jwtService.ValidateToken(token);
            if (principal == null)
            {
                return new JsonResult(new { success = false, message = "Token không hợp lệ hoặc hết hạn." }) { StatusCode = 401 };
            }

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return new JsonResult(new { success = false, message = "Không tìm thấy thông tin người dùng." }) { StatusCode = 401 };
            }

            if (string.IsNullOrEmpty(Challenge_id))
            {
                return new JsonResult(new { success = false, message = "Challenge ID không hợp lệ." }) { StatusCode = 400 };
            }
            
            var requestData = new StartGameRequest 
            {
                ChallengeId = Challenge_id,
                UserId = userId,
                PlayerCount = 1
            };

            try
            {
                string apiUrl = $"{ApiBaseUrl}Game/Start";
                var jsonContent = new StringContent(JsonSerializer.Serialize(requestData), 
                                                    System.Text.Encoding.UTF8, "application/json");
                
                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    string jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    
                    SessionData = JsonSerializer.Deserialize<GameSessionStarted>(jsonString, options);
                    
                    GameStarted = true; 

                    return new JsonResult(new { 
                        success = true, 
                        takenId = SessionData.TakenId,
                        startTime = SessionData.StartTimeUtc,
                        deadline = SessionData.DeadlineUtc,
                        maxDuration = SessionData.MaxDurationSeconds
                    });
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    return new JsonResult(new { success = false, message = $"Lỗi Server: {response.StatusCode} - {error}" }) { StatusCode = (int)response.StatusCode };
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Lỗi hệ thống: {ex.Message}" }) { StatusCode = 500 };
            }
        }
    }
}