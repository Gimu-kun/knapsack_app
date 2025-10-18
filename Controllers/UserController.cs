using Microsoft.AspNetCore.Mvc;
using knapsack_app.ViewModels;

namespace knapsack_app.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// API Đăng nhập người dùng.
        /// </summary>
        /// <param name="request">Chứa Account và Password.</param>
        /// <returns>Token JWT nếu thành công hoặc thông báo lỗi.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginReqDto request)
        {
            if (string.IsNullOrEmpty(request.Account) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new AuthResult { Success = false, Message = "Tài khoản và mật khẩu không được để trống." });
            }

            // Gọi service để xác thực
            var authResult = await _userService.Login(request);
            
            if (authResult.Success)
            {
                // Trả về token và thông báo thành công
                return Ok(authResult);
            }
            else
            {
                // Trả về 401 Unauthorized nếu thất bại
                return Unauthorized(authResult); 
            }
        }


        // --- CÁC PHƯƠNG THỨC ĐIỀU KHIỂN USER KHÁC (Đã có sẵn) ---

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
    }
}