using knapsack_app.Services;
using knapsack_app.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace knapsack_app.Pages.Admin
{
    public class ChallengeManagerModel : PageModel
    {
        private readonly ChallengeService _challengeService;
        private readonly JwtService _jwtService;
        private const int DEFAULT_PAGE_SIZE = 10; 

        // Dependency Injection
        public ChallengeManagerModel(ChallengeService challengeService, JwtService jwtService)
        {
            _challengeService = challengeService;
            _jwtService = jwtService;
        }

        // --- Thuộc tính cho Razor Page ---

        // Dữ liệu hiển thị trên bảng
        public IEnumerable<ChallengeViewModel> Challenges { get; set; } = new List<ChallengeViewModel>();

        // Phân trang
        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1;

        public int TotalPages { get; set; } = 0;
        
        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = DEFAULT_PAGE_SIZE;

        // Tìm kiếm
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;
        
        public string adminId { get; set; } = string.Empty;

        // --- Xử lý GET Request (Tải dữ liệu) ---
        public async Task OnGetAsync()
        {
            string? adminToken = Request.Cookies["AdminToken"];
            var role = _jwtService.ValidateToken(adminToken);

            adminId = role?.FindFirst("id")?.Value ?? string.Empty;

            if (role == null)
            {
                adminId = string.Empty;
                Response.Redirect("/Admin/Login");
                return;
            }

            // 1. Đảm bảo các giá trị phân trang hợp lệ
            if (PageIndex < 1) PageIndex = 1;
            if (PageSize < 1) PageSize = DEFAULT_PAGE_SIZE;

            try
            {
                // 2. Gọi Service để lấy dữ liệu
                var (challenges, totalCount) = await _challengeService
                    .GetPaginatedChallengesAsync(PageIndex, PageSize, SearchTerm);

                Challenges = challenges;

                // 3. Tính toán tổng số trang
                TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
                
                // 4. Xử lý trường hợp người dùng truy cập trang quá giới hạn
                if (PageIndex > TotalPages && TotalPages > 0)
                {
                    PageIndex = TotalPages;
                    // Tùy chọn: Gọi lại Service với PageIndex đã sửa để tránh trang trắng
                    (challenges, totalCount) = await _challengeService
                        .GetPaginatedChallengesAsync(PageIndex, PageSize, SearchTerm);
                    Challenges = challenges;
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi và thông báo cho người dùng
                // TODO: Log the exception (using ILogger)
                TempData["ErrorMessage"] = $"Lỗi khi tải danh sách đề bài: {ex.Message}";
                Challenges = new List<ChallengeViewModel>();
                TotalPages = 0;
            }
        }

        // --- Xử lý Delete (sử dụng Handler) ---
        // Thêm tham số phân trang vào handler để đảm bảo chuyển hướng về đúng trang
        public async Task<IActionResult> OnGetDeleteAsync(string id, int pageIndex, int pageSize, string searchTerm)
        {
            // Cập nhật lại các thuộc tính phân trang/tìm kiếm
            PageIndex = pageIndex;
            PageSize = pageSize;
            SearchTerm = searchTerm;
            
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Không tìm thấy ID đề bài để xóa.";
                return RedirectToPage("./ChallengeManager", new { PageIndex, PageSize, SearchTerm });
            }

            try
            {
                await _challengeService.DeleteChallengeAsync(id);
                TempData["SuccessMessage"] = $"Đã xóa đề bài ID '{id}' thành công!";
            }
            catch (Exception ex)
            {
                // Ghi log lỗi và thông báo cho người dùng
                // TODO: Log the exception (using ILogger)
                TempData["ErrorMessage"] = $"Lỗi khi xóa đề bài ID '{id}': {ex.Message}";
            }

            // Chuyển hướng trở lại trang hiện tại sau khi xóa
            // Sử dụng các giá trị đã được truyền vào (pageIndex, pageSize, searchTerm)
            return RedirectToPage("./ChallengeManager", new { PageIndex, PageSize, SearchTerm });
        }
    }
}