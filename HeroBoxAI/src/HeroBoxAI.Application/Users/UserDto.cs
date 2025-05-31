using System;
using HeroBoxAI.Domain.Enums;

namespace HeroBoxAI.Application.Users;

public record UserDto
{
    public Guid Id { get; init; }
    public string Username { get; init; }
    public string Email { get; init; }
    public UserRole Role { get; init; }
    public UserStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid? ClanId { get; init; }
    public string? ClanName { get; init; }
    public int HeroCount { get; init; }
} 