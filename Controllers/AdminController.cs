using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace knapsack_app.Controllers
{
    [ApiController]
    [Route("/api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly AdminService _adminService;
        private readonly JwtService _jwtTokenService;

        public AdminController(AdminService adminService, JwtService jwtTokenService)
        {
            _adminService = adminService;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAd([FromForm] AdminCreationReqDto request)
        {
            var result = await _adminService.CreateAccount(request);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Message);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAdmins()
        {
            var admins = await _adminService.GetAllAdmins();
            return Ok(admins);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAdminById(string id)
        {
            var admin = await _adminService.GetAdminById(id);
            if (admin == null)
            {
                return NotFound("Admin not found");
            }
            return Ok(admin);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAdmin(string id, [FromBody] AdminUpdateReqDto request)
        {
            var result = await _adminService.UpdateAdmin(id, request);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Message);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdmin(string id)
        {
            var result = await _adminService.DeleteAdmin(id);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Message);
        }

        // Đăng nhập admin
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _adminService.Login(request);
            if (!result.Success || result.Admin == null)
            {
                return BadRequest(result.Message);
            }
            var token = _jwtTokenService.GenerateToken(result.Admin.Id, result.Admin.Account, result.Admin.Role);
            return Ok(new
            {
                result.Message,
                token
            });
        }
    }
}