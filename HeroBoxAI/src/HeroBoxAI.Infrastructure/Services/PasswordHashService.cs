using HeroBoxAI.Application.Common.Interfaces;

namespace HeroBoxAI.Infrastructure.Services;

public class PasswordHashService : IPasswordHashService
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
} 