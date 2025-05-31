using System;
using MediatR;

namespace HeroBoxAI.Application.Users.Commands.UpdateCurrentUser;

public record UpdateCurrentUserCommand : IRequest<UserDto>
{
    public Guid UserId { get; init; }
    public string Username { get; init; }
    public string Email { get; init; }
} 