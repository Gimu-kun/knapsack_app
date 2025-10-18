public class UserCreationReqDto
{
    public string Account { get; set; }
    public string Passwords { get; set; }
    public IFormFile? Avatar { get; set; }
}

public class UserUpdateReqDto
{
    public string? Account { get; set; }
    public string? Passwords { get; set; }
    public string? Avatar { get; set; }
    public bool? Status { get; set; }
}

public class AuthResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public string? UserId { get; set; }
    public bool IsAdmin { get; set; } = false;
}

public class LoginReqDto
{
    public string Account { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}