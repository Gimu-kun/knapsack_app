using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations; // Thêm thư viện này
using Microsoft.AspNetCore.Http; // Thêm thư viện này cho CookieOptions

namespace knapsack_app.Pages.Admin
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public LoginRequest LoginRequest { get; set; } = new LoginRequest();
        public string? ErrorMessage { get; set; } = string.Empty;

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();  
            }
            
            using var client = new HttpClient();
            var json = JsonSerializer.Serialize(LoginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("http://localhost:5238/api/admin/login", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                
                try
                {
                    var doc = JsonDocument.Parse(responseBody);
                    var token = doc.RootElement.GetProperty("token").GetString();

                    Response.Cookies.Append("AdminToken", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddHours(6)
                    });

                    return RedirectToPage("/Admin/DashBoard");
                }
                catch (JsonException)
                {
                    ErrorMessage = "Lỗi phản hồi từ API: Không tìm thấy 'token'.";
                    return Page();
                }
                catch (KeyNotFoundException)
                {
                    ErrorMessage = "Lỗi phản hồi từ API: Cấu trúc JSON không hợp lệ, không có trường 'token'.";
                    return Page();
                }
            }
            else
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                ErrorMessage = $"Đăng nhập thất bại! {responseBody}";
                return Page();
            }
        }
    }
}