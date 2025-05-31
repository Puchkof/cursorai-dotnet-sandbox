using MediatR;

namespace HeroBoxAI.Application.Users.Commands.SignIn;

public record SignInCommand : IRequest<AuthResponseDto>
{
    public string? Email { get; init; }
    public string? Username { get; init; }
    public string Password { get; init; }
} 