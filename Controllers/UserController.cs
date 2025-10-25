using Microsoft.AspNetCore.Mvc;
using knapsack_app.ViewModels;

namespace knapsack_app.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly UserProgressService _progressService;

        public UserController(UserService userService, UserProgressService progressService)
        {
            _userService = userService;
            _progressService = progressService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginReqDto request)
        {
            if (string.IsNullOrEmpty(request.Account) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new AuthResult { Success = false, Message = "Tài khoản và mật khẩu không được để trống." });
            }

            var authResult = await _userService.Login(request);
            
            if (authResult.Success)
            {
                return Ok(authResult);
            }
            else
            {
                return Unauthorized(authResult); 
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromForm] UserCreationReqDto request)
        {
            var result = await _userService.CreateUser(request);
            if (!result.Success)
                return BadRequest(result.Message);
            return Ok(result.Message);
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<UserManagementDto>>> GetPaginatedUsers([FromQuery] UserQueryRequestDto query)
        {
            var users = await _userService.GetPaginatedUsersAsync(query);
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userService.GetUserById(id);
            if (user == null)
                return NotFound("User not found");
            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateReqDto request)
        {
            var result = await _userService.UpdateUser(id, request);
            if (!result.Success)
                return BadRequest(result.Message);
            return Ok(result.Message);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _userService.DeleteUser(id);
            if (!result.Success)
                return BadRequest(result.Message);
            return Ok(result.Message);
        }

        [HttpGet("{userId}/progress")]
        [ProducesResponseType(typeof(UserProgressDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserProgress(string userId)
        {
            var progress = await _progressService.GetUserProgressAsync(userId);

            if (progress == null)
            {
                return NotFound($"Không tìm thấy người dùng với ID: {userId}.");
            }

            return Ok(progress);
        }

        [HttpPut("{id}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            var result = await _userService.ToggleUserStatus(id);
            
            if (!result.Success)
            {
                return BadRequest(new { Success = false, result.Message }); 
            }
            
            return Ok(new 
            { 
                Success = true, 
                result.Message, 
                NewStatus = result.NewStatus 
            });
        }
    }
}