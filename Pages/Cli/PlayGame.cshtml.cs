using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace knapsack_app.Pages.Cli
{
    public class PlayGameModel : PageModel
    {
        private readonly HttpClient _httpClient;
        
        // Đường dẫn API cơ bản
        private const string ApiBaseUrl = "http://localhost:5238/api/Challenge/";

        // Inject HttpClient thông qua constructor (Cần đăng ký trong Program.cs)
        public PlayGameModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // ======================================================================
        // 1. DỮ LIỆU ĐẦU VÀO TỪ SERVER (INPUT DATA)
        // ======================================================================

        [BindProperty(SupportsGet = true)] // Cho phép Model binding từ Query String
        public int Players { get; set; } = 1;

        [BindProperty(SupportsGet = true)] // Cho phép Model binding từ Query String
        public int Difficulty { get; set; } = 1;

        // Thuộc tính để lưu trữ ID thử thách, cần thiết cho API call
        [BindProperty(SupportsGet = true)]
        public string Challenge_id { get; set; } = string.Empty;
        
        public GameData InputGameData { get; set; } = new GameData();
        public List<KnapsackItem> Items { get; set; } = new List<KnapsackItem>();
        public int[,] DPBoard { get; set; } = new int[0, 0];

        // ======================================================================
        // 2. LOGIC XỬ LÝ: LẤY DỮ LIỆU TỪ API
        // ======================================================================

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(Challenge_id))
            {
                // Xử lý trường hợp không có ID (ví dụ: chuyển hướng hoặc trả về 404)
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
                    // Deserialize dữ liệu JSON chính
                    InputGameData = JsonSerializer.Deserialize<GameData>(jsonString, options) ?? new GameData();

                    // Parse các chuỗi JSON lồng nhau
                    ParseNestedJson();
                    
                    // Gán tên vật phẩm giả lập (A, B, C...)
                    AssignItemNames();

                    return Page();
                }
                else
                {
                    // Xử lý lỗi API (ví dụ: trả về trang lỗi với mã trạng thái)
                    ModelState.AddModelError(string.Empty, $"Lỗi khi tải dữ liệu thử thách. Mã lỗi: {response.StatusCode}");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi kết nối hoặc ngoại lệ khác
                ModelState.AddModelError(string.Empty, $"Lỗi hệ thống: {ex.Message}");
                return Page();
            }
        }

        private void ParseNestedJson()
        {
            // 1. Parse Items Data (danh sách vật phẩm)
            if (!string.IsNullOrEmpty(InputGameData.QuesDataJson))
            {
                Items = JsonSerializer.Deserialize<List<KnapsackItem>>(InputGameData.QuesDataJson);
            }

            // 2. Parse DP Board (Bảng Quy Hoạch Động)
            if (!string.IsNullOrEmpty(InputGameData.DpBoardJson))
            {
                var tempList = JsonSerializer.Deserialize<List<List<int>>>(InputGameData.DpBoardJson);
                if (tempList != null && tempList.Count > 0)
                {
                    int rows = tempList.Count;
                    int cols = tempList.First().Count;
                    DPBoard = new int[rows, cols];

                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < cols; j++)
                        {
                            DPBoard[i, j] = tempList[i][j];
                        }
                    }
                }
            }
        }
        
        private void AssignItemNames()
        {
            // Gán tên giả lập (A, B, C...) cho các vật phẩm để hiển thị trên UI
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].Name = ((char)('A' + i)).ToString();
            }
        }
    }

    // ======================================================================
    // 3. CÁC LỚP ĐỊNH NGHĨA DỮ LIỆU (Giữ nguyên)
    // ======================================================================

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

    public class KnapsackItem
    {
        public string Id { get; set; } = string.Empty;
        public int Value { get; set; }
        public int Weight { get; set; }
        public string Name { get; set; } = "Item"; 
    }
}