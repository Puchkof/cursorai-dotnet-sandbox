using System;
using HeroBoxAI.Domain.Enums;

namespace HeroBoxAI.Application.Clans;

public record ClanMemberDto
{
    public Guid Id { get; init; }
    public string Username { get; init; }
    public string Email { get; init; }
    public UserRole Role { get; init; }
    public UserStatus Status { get; init; }
    public DateTime JoinedAt { get; init; }
    public int HeroCount { get; init; }
} 