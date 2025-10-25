using knapsack_app.ViewModels;
using knapsack_app.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace knapsack_app.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChallengeController : ControllerBase
    {
        private readonly ChallengeService _challengeService;
        private readonly AdminService _adminService;

        public ChallengeController(ChallengeService challengeService, AdminService adminService)
        {
            _challengeService = challengeService;
            _adminService = adminService;
        }

        private class ChallengeApiViewModel
        {
            public string Id { get; set; } = string.Empty;
            public int Difficulty { get; set; }
            public int MaxCapacity { get; set; }
            public int MaxDuration { get; set; }
            public int MissCount { get; set; }
            public DateTime CreatedAt { get; set; }
            public string CreatedBy { get; set; } = "ADMIN";
        }

        [HttpGet]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetPaginatedChallenges(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = "")
        {
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

        [HttpPost]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Conflict)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateChallenge([FromBody] ChallengeCreateEditModel model)
        {
            var Operator = await _adminService.GetAdminById(model.OperatorId);
            
            if (string.IsNullOrEmpty(Operator.Account))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {

                var newId = await _challengeService.CreateChallengeAsync(model,Operator.Id);

                return CreatedAtAction(nameof(GetChallengeById), new { id = newId }, new { Id = newId, Message = "Tạo đề bài thành công. Bảng DP và lỗ hổng đã được tính toán tự động." });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                var baseException = ex.GetBaseException();
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    Message = $"Đã xảy ra lỗi server khi tạo đề bài. {baseException.Message}",
                    Detail = baseException.ToString()
                });
            }
        }

        [HttpPut]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateChallenge([FromBody] ChallengeCreateEditModel model)
        {
            var Operator = await _adminService.GetAdminById(model.OperatorId);
            if (string.IsNullOrEmpty(Operator.Account))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _challengeService.UpdateChallengeAsync(model,Operator.Id);
                return NoContent();
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

        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> DeleteChallenge(string id)
        {
            try
            {
                await _challengeService.DeleteChallengeAsync(id);
                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new { Message = $"Đã xảy ra lỗi khi xóa đề bài ID '{id}'." });
            }
        }

        [HttpGet("random")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetRandomOrSpecificChallenge(
        [FromQuery] int difficulty,
        [FromQuery] string? challengeId = null)
        {
            if (!Enum.IsDefined(typeof(DifficultyEnum), difficulty))
            {
                return BadRequest($"Giá trị độ khó '{difficulty}' không hợp lệ. Vui lòng sử dụng 1 (easy), 2 (medium), hoặc 3 (hard).");
            }

            var difficultyEnum = (DifficultyEnum)difficulty;

            try
            {
                var challenge = await _challengeService
                    .GetRandomOrSpecificChallengeAsync(difficultyEnum, challengeId);

                if (challenge == null)
                {
                    if (!string.IsNullOrEmpty(challengeId))
                    {
                        return NotFound($"Không tìm thấy đề bài với ID '{challengeId}' có độ khó '{difficultyEnum}'.");
                    }
                    return NotFound($"Hiện tại chưa có đề bài nào với độ khó '{difficultyEnum}'.");
                }

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
            catch (Exception ex)
            {
                var baseException = ex.GetBaseException();
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    Message = $"Đã xảy ra lỗi server khi lấy đề bài. {baseException.Message}"
                });
            }
        }
    }
}