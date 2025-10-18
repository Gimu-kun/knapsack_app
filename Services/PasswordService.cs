using Microsoft.AspNetCore.Identity;

public class PasswordService
{
    public readonly PasswordHasher<string> _passwordHasher = new PasswordHasher<string>();

    public string hashPassword(string passwords)
    {
        return _passwordHasher.HashPassword(null, passwords);
    }

    public bool verifyPassword(string hashedPassword, string rawPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(null, hashedPassword, rawPassword);
        return result == PasswordVerificationResult.Success;
    }
}