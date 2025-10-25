using System.Security.Claims;
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

        public ChallengeManagerModel(ChallengeService challengeService, JwtService jwtService)
        {
            _challengeService = challengeService;
            _jwtService = jwtService;
        }

        public IEnumerable<ChallengeViewModel> Challenges { get; set; } = new List<ChallengeViewModel>();

        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1;

        public int TotalPages { get; set; } = 0;
        
        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = DEFAULT_PAGE_SIZE;

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;
        
        public string adminId { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            string? adminToken = Request.Cookies["AdminToken"];
            var role = _jwtService.ValidateToken(adminToken);

            adminId = role?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            if (role == null)
            {
                adminId = string.Empty;
                Response.Redirect("/Admin/Login");
                return;
            }

            if (PageIndex < 1) PageIndex = 1;
            if (PageSize < 1) PageSize = DEFAULT_PAGE_SIZE;

            try
            {
                var (challenges, totalCount) = await _challengeService
                    .GetPaginatedChallengesAsync(PageIndex, PageSize, SearchTerm);

                Challenges = challenges;

                TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
                
                if (PageIndex > TotalPages && TotalPages > 0)
                {
                    PageIndex = TotalPages;
                    (challenges, totalCount) = await _challengeService
                        .GetPaginatedChallengesAsync(PageIndex, PageSize, SearchTerm);
                    Challenges = challenges;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tải danh sách đề bài: {ex.Message}";
                Challenges = new List<ChallengeViewModel>();
                TotalPages = 0;
            }
        }

        public async Task<IActionResult> OnGetDeleteAsync(string id, int pageIndex, int pageSize, string searchTerm)
        {
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
                TempData["ErrorMessage"] = $"Lỗi khi xóa đề bài ID '{id}': {ex.Message}";
            }

            return RedirectToPage("./ChallengeManager", new { PageIndex, PageSize, SearchTerm });
        }
    }
}