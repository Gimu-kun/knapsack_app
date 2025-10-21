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

namespace knapsack_app.Pages.Cli
{
    // ======================================================================
    // 3. CÁC LỚP ĐỊNH NGHĨA DỮ LIỆU
    // ======================================================================
    

    public class PlayGameModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly JwtService _jwtService;
        private const string ApiBaseUrl = "http://localhost:5238/api/Challenge/"; 

        public PlayGameModel(HttpClient httpClient, JwtService jwtService)
        {
            _httpClient = httpClient;
            _jwtService = jwtService;
        }

        // 1. DỮ LIỆU ĐẦU VÀO TỪ SERVER (INPUT DATA)
        [BindProperty(SupportsGet = true)]
        public string Challenge_id { get; set; } = string.Empty;
        [BindProperty(SupportsGet = true)]
        public string? RoomId { get; set; } = string.Empty;
        [BindProperty(SupportsGet = true)]
        public string? TeamId { get; set; } = string.Empty;
        [BindProperty(SupportsGet = true)]
        public int? Players { get; set; } = 1;
        [BindProperty(SupportsGet = true)]
        public string? TeamName { get; set; } = string.Empty;
        public GameSessionStarted SessionData { get; set; } = new GameSessionStarted();
        public GameData InputGameData { get; set; } = new GameData();
        public List<KnapsackItem> Items { get; set; } = new List<KnapsackItem>();
        public int[,] DPBoard { get; set; } = new int[1, 1]; 
        
        // Dữ liệu parse từ ResultItemsJson
        public List<KnapsackItem> ResultItems { get; set; } = new List<KnapsackItem>();
        // Dữ liệu parse từ MissArrayJson
        public List<MissCellDto> MissArray { get; set; } = new List<MissCellDto>(); 

        // --- CẬP NHẬT TRẠNG THÁI TỐI GIẢN ---
        public bool GameStarted { get; set; } = false; 
        
        // Thuộc tính hỗ trợ Razor (Chỉ số ma trận DP)
        public int dpRows => DPBoard.GetLength(0);
        public int dpCols => DPBoard.GetLength(1);
        public int capacity => InputGameData.MaxCapacity;
        public string? userId;
        
        // Số lượng ô cần người dùng điền (không tính hàng 0 và cột 0)
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

        // Tổng số ô trong ma trận DP (bao gồm cả hàng 0 và cột 0)
        public int TotalInputCells => dpRows * dpCols; 
        // ------------------------------------

        // 2. LOGIC XỬ LÝ: LẤY DỮ LIỆU TỪ API
        public async Task<IActionResult> OnGetAsync()
        {
            // 1. Lấy JWT từ cookie
            if (!HttpContext.Request.Cookies.TryGetValue("UserToken", out var token) || string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Cli/Login");
            }

            // 2. Xác thực token bằng JwtService
            var principal = _jwtService.ValidateToken(token);
            if (principal == null)
            {
                // Token không hợp lệ hoặc hết hạn
                return RedirectToPage("/Cli/Login");
            }

            // 3. Lấy UserId từ claim
            userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Cli/Login");
            }

            // ✅ Đến đây, bạn đã có userId hợp lệ để sử dụng
            // Ví dụ: truyền vào API hoặc dùng để log
            Console.WriteLine("Đăng nhập thành công với userId: " + userId);
    
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

        /**
         * Phương thức nội bộ để phân tích cú pháp các chuỗi JSON lồng nhau
         */
        private void ParseNestedJson()
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            
            // 1. Parse Items Data (Danh sách vật phẩm câu hỏi)
            if (!string.IsNullOrEmpty(InputGameData.QuesDataJson))
            {
                Items = JsonSerializer.Deserialize<List<KnapsackItem>>(InputGameData.QuesDataJson, options) ?? new List<KnapsackItem>();
            }

            // 2. Parse DP Board (Ma trận quy hoạch động)
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
            
            // 3. Parse Result Items (Danh sách vật phẩm kết quả tối ưu)
            if (!string.IsNullOrEmpty(InputGameData.ResultItemsJson))
            {
                ResultItems = JsonSerializer.Deserialize<List<KnapsackItem>>(InputGameData.ResultItemsJson, options) ?? new List<KnapsackItem>();
            }
            
            // 4. Parse Miss Array (Danh sách các ô bị lỗi/sai)
            // SỬ DỤNG LỚP CELLCOORDINATE ĐỂ PHÙ HỢP VỚI CẤU TRÚC JSON MỚI (X, Y)
            if (!string.IsNullOrEmpty(InputGameData.MissArrayJson))
            {
                MissArray = JsonSerializer.Deserialize<List<MissCellDto>>(InputGameData.MissArrayJson, options) ?? new List<MissCellDto>();
            }
        }

        /**
         * Phương thức nội bộ để gán tên thân thiện (Item A, Item B, ...)
         */
        private void AssignItemNames()
        {
            // Gán tên cho danh sách Item chính
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].Name = $"Item {((char)('A' + i)).ToString()}";
                Items[i].Id = (i + 1).ToString();
            }

            // Cập nhật tên cho ResultItems (nếu có) để nhất quán
            foreach (var resultItem in ResultItems)
            {
                // Giả sử Id là GUID trong JSON, chúng ta cần tìm Id khớp với GUID
                // Tuy nhiên, vì chúng ta đã gán Id là index (1, 2, 3...) cho Items,
                // chúng ta chỉ có thể dựa vào GUID gốc nếu cần mapping chính xác.
                // Ở đây, tôi giữ nguyên logic mapping để đảm bảo Name được gán chính xác.
                var matchingItem = Items.FirstOrDefault(i => i.Id == resultItem.Id);
                if (matchingItem != null)
                {
                    resultItem.Name = matchingItem.Name;
                }
                else
                {
                    // Nếu không tìm thấy bằng ID đã gán (1, 2, 3...), thử tìm bằng Value/Weight
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

        // 3. XỬ LÝ KHI NGƯỜI DÙNG NHẤN NÚT START GAME (Tạo phiên chơi trên Server)
        public async Task<IActionResult> OnPostStartGameAsync()
        {
            if (!HttpContext.Request.Cookies.TryGetValue("UserToken", out var token) || string.IsNullOrEmpty(token))
            {
                return new JsonResult(new { success = false, message = "Người dùng chưa đăng nhập." }) { StatusCode = 401 };
            }

            // 2. Xác thực
            var principal = _jwtService.ValidateToken(token);
            if (principal == null)
            {
                return new JsonResult(new { success = false, message = "Token không hợp lệ hoặc hết hạn." }) { StatusCode = 401 };
            }

            // 3. Lấy userId
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
                PlayerCount = 1 // Giả định solo
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
                    
                    // Lưu Session Data trả về từ Server
                    SessionData = JsonSerializer.Deserialize<GameSessionStarted>(jsonString, options);
                    
                    // Bật trạng thái game đã bắt đầu
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