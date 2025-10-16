public class UserCreationReqDto
{
    public string Account { get; set; }
    public string Passwords { get; set; }
    public string? Avatar { get; set; }
}

public class UserUpdateReqDto
{
    public string? Account { get; set; }
    public string? Passwords { get; set; }
    public string? Avatar { get; set; }
    public bool? Status { get; set; }
}