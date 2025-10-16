using knapsack_app.ViewModels;
using knapsack_app.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net; 
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic; // Cần thêm namespace này

namespace knapsack_app.Controllers
{
    // Đổi tên thành ChallengeController cho nhất quán
    [Route("api/[controller]")] 
    [ApiController]
    public class ChallengeController : ControllerBase
    {
        private readonly ChallengeService _challengeService;
        
        // ĐÃ LOẠI BỎ: private const string CurrentUserId = "API_Admin"; 

        public ChallengeController(ChallengeService challengeService)
        {
            _challengeService = challengeService;
        }

        // DTO nội bộ để trả về danh sách, đảm bảo Difficulty là int
        private class ChallengeApiViewModel
        {
            public string Id { get; set; } = string.Empty;
            public int Difficulty { get; set; } // Cast DifficultyEnum sang int
            public int MaxCapacity { get; set; }
            public int MaxDuration { get; set; }
            public int MissCount { get; set; }
            public DateTime CreatedAt { get; set; }
            public string CreatedBy { get; set; } = "ADMIN"; 
        }

        // --- 1. Lấy danh sách đề bài (GET) ---
        [HttpGet]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetPaginatedChallenges(
            [FromQuery] int pageIndex = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? searchTerm = "")
        {
            // Thêm kiểm tra validation đơn giản cho tham số query
            if (pageIndex < 1 || pageSize < 1)
            {
                return BadRequest("Tham số phân trang không hợp lệ.");
            }

            var (challenges, totalCount) = await _challengeService
                .GetPaginatedChallengesAsync(pageIndex, pageSize, searchTerm ?? string.Empty);

            var apiChallenges = challenges.Select(c => new ChallengeApiViewModel
            {
                Id = c.Id,
                Difficulty = (int)c.Difficulty, 
                MaxCapacity = c.MaxCapacity,
                MaxDuration = c.MaxDuration,
                MissCount = c.MissCount,
                CreatedAt = c.CreatedAt,
                CreatedBy = c.CreatedBy
            }).ToList();

            return Ok(new 
            {
                Challenges = apiChallenges,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            });
        }

        // --- 2. Lấy chi tiết đề bài (GET {id}) ---
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetChallengeById(string id)
        {
            var challenge = await _challengeService.GetChallengeByIdAsync(id);

            if (challenge == null)
            {
                return NotFound($"Không tìm thấy đề bài với ID: {id}");
            }
            
            // Trả về đối tượng ẩn danh để đảm bảo Difficulty luôn là int trong JSON response
            return Ok(new 
            {
                challenge.Id,
                Difficulty = (int)challenge.Difficulty,
                challenge.MaxCapacity,
                challenge.MaxDuration,
                challenge.MissCount,
                challenge.QuesDataJson,
                challenge.DpBoardJson,
                challenge.ResultItemsJson,
                challenge.MissArrayJson
            });
        }
        
        // --- 3. TẠO đề bài mới (POST) ---
        
        /// <summary>
        /// Tạo đề bài mới. Tự động tính toán bảng DP, kết quả và lỗ hổng.
        /// POST: api/Challenge
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.Created)] // Trả về ID mới
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateChallenge([FromBody] ChallengeCreateEditModel model)
        {
            Console.WriteLine("[DEBUG] Bắt đầu xử lý CreateChallenge");
            Console.WriteLine($"[DEBUG] Model nhận được: {System.Text.Json.JsonSerializer.Serialize(model)}");
            var adminId = User.FindFirst("id")?.Value ?? "AD-2D9E4F6A";
            Console.WriteLine($"[DEBUG] Admin ID from token: {adminId}");
            if (string.IsNullOrEmpty(adminId))
            {
                return Unauthorized();
            }

            // 1. Model Validation
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Trả về 400 Bad Request với chi tiết lỗi
            }

            try
            {
                // 2. Gọi Service, sử dụng ID người tạo được truyền vào từ body
                var newId = await _challengeService.CreateChallengeAsync(model, adminId); 

                // 3. Phản hồi thành công
                // Trả về 201 Created và link để lấy chi tiết tài nguyên mới tạo
                return CreatedAtAction(nameof(GetChallengeById), new { id = newId }, new { Id = newId, Message = "Tạo đề bài thành công. Bảng DP và lỗ hổng đã được tính toán tự động." });
            }
            catch (InvalidOperationException ex)
            {
                // Xử lý lỗi logic nghiệp vụ: ID đã tồn tại, lỗi Deserialize JSON, v.v.
                return Conflict(new { Message = ex.Message }); // 409 Conflict
            }
            catch (Exception ex)
            {
                // Xử lý lỗi hệ thống chung
                var baseException = ex.GetBaseException();
                return StatusCode((int)HttpStatusCode.InternalServerError, new 
                { 
                    Message = $"Đã xảy ra lỗi server khi tạo đề bài. {baseException.Message}",
                    Detail = baseException.ToString()
                });
            }
        }

        // --- 4. Cập nhật đề bài (PUT) ---
        
        /// <summary>
        /// Cập nhật đề bài hiện có.
        /// PUT: api/Challenge
        /// </summary>
        [HttpPut]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateChallenge([FromBody] ChallengeCreateEditModel model)
        {
            var adminId = User.FindFirst("id")?.Value;
            Console.WriteLine($"[DEBUG] Admin ID from token: {adminId}");
            if (string.IsNullOrEmpty(adminId))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Sử dụng ID người tạo/cập nhật được truyền vào từ body
                await _challengeService.UpdateChallengeAsync(model, adminId);
                return NoContent(); // 204 No Content
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { Message = "Đã xảy ra lỗi server khi cập nhật đề bài." });
            }
        }

        // --- 5. Xóa đề bài (DELETE {id}) ---
        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeleteChallenge(string id)
        {
            try
            {
                await _challengeService.DeleteChallengeAsync(id);
                // Trả về 204 ngay cả khi không tìm thấy, vì mục tiêu là đảm bảo nó bị xóa (Idempotency)
                return NoContent(); 
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { Message = $"Đã xảy ra lỗi khi xóa đề bài ID '{id}'." });
            }
        }
    }
}