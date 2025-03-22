using System;

namespace HeroBoxAI.Application.Clans;

public record ClanDto
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Tag { get; init; }
    public string Description { get; init; }
    public int Level { get; init; }
    public Guid FounderId { get; init; }
    public string FounderName { get; init; }
    public int MemberCount { get; init; }
    public DateTime CreatedAt { get; init; }
} 