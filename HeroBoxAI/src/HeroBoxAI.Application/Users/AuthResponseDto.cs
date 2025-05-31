namespace HeroBoxAI.Application.Users;

public record AuthResponseDto
{
    public string Token { get; init; }
    public UserDto User { get; init; }
} 