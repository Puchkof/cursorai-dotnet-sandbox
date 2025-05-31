using MediatR;

namespace HeroBoxAI.Application.Users.Commands.SignUp;

public record SignUpCommand : IRequest<AuthResponseDto>
{
    public string Username { get; init; }
    public string Email { get; init; }
    public string Password { get; init; }
} 