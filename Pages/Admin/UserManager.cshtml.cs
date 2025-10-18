using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using knapsack_app.ViewModels;
using knapsack_app.ViewModels;

namespace knapsack_app.Pages.Admin
{
    public class EditUserRequestDto 
    {
        public int UserId { get; set; }
        public bool Status { get; set; }
        public string NewPassword { get; set; }
    }

    public class UserManagerModel : PageModel
    {
        public List<UserViewModel> Users { get; set; } = new List<UserViewModel>();
        
        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        
        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;
        
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;

        [BindProperty]
        public EditUserRequestDto EditUserInput { get; set; }


        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl;

        public UserManagerModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl")?.TrimEnd('/') ?? "http://localhost:5238";
        }

        public async Task OnGetAsync()
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_apiBaseUrl); 

            var apiQuery = new UserQueryRequestDto
            {
                PageIndex = PageIndex,
                PageSize = PageSize,
                SearchTerm = SearchTerm
            };

            var apiPath = $"/api/user?pageIndex={apiQuery.PageIndex}&pageSize={apiQuery.PageSize}&searchTerm={apiQuery.SearchTerm}";
            Console.WriteLine($"[DEBUG] Gọi API tại: {httpClient.BaseAddress}{apiPath}");

            try
            {
                var response = await httpClient.GetFromJsonAsync<PaginatedResponse<UserViewModel>>(apiPath);
                if (response != null)
                {
                    Users = response.Data ?? new List<UserViewModel>();
                    TotalPages = response.TotalPages;
                    PageIndex = response.PageIndex;
                    PageSize = response.PageSize;
                }
                else
                {
                    Users = new List<UserViewModel>();
                    TotalPages = 1;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[HTTP ERROR] Lỗi khi gọi API tại {apiPath}: {ex.Message}");
                TempData["ErrorMessage"] = "Không thể kết nối tới dịch vụ quản lý người dùng.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FATAL ERROR] Lỗi không xác định: {ex.Message}");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi không mong muốn trong quá trình tải dữ liệu.";
            }
        }


        public async Task<IActionResult> OnGetPlayHistoryAsync(string userId)
        {
            Console.WriteLine($"[DEBUG] Tải lịch sử chơi cho User ID: {userId}");
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_apiBaseUrl); 
            
            var apiPath = $"api/admin/history/{userId}"; 
            Console.WriteLine($"[DEBUG] Gọi API lịch sử chơi tại: {httpClient.BaseAddress}{apiPath}");
            try
            {
                var response = await httpClient.GetFromJsonAsync<List<PlayHistoryApiViewModel>>($"{_apiBaseUrl}/{apiPath}");
                Console.WriteLine($"[DEBUG] Phản hồi từ API lịch sử chơi: {response?.Count ?? 0} mục");
                if (response == null)
                {
                    return new JsonResult(new List<PlayHistoryApiViewModel>());
                }
                
                return new JsonResult(response);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[HTTP ERROR] Lỗi khi gọi API lịch sử chơi: {ex.Message}");
                return new StatusCodeResult(500);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FATAL ERROR] Lỗi không xác định khi tải lịch sử: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}