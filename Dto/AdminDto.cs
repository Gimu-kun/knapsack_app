using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

public class AdminCreationReqDto
{
    public string Account { get; set; }
    public string Passwords { get; set; }
    public IFormFile? Avatar { get; set; }
    public string? CreatedBy { get; set; }
}

public class AdminUpdateReqDto
{
    public string? Account { get; set; }
    public string? Passwords { get; set; }
    public string? Avatar { get; set; }
    public bool? Role { get; set; }
}

public class LoginRequest
{
    [Required(ErrorMessage = "Tài khoản không được để trống.")]
    public string Account { get; set; }
    [Required(ErrorMessage = "Mật khẩu không được để trống.")]
    public string Passwords { get; set; }
}