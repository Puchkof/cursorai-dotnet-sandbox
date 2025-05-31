using System;
using MediatR;

namespace HeroBoxAI.Application.Clans.Commands.UpdateClan;

public record UpdateClanCommand : IRequest<ClanDto>
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Tag { get; init; }
    public string Description { get; init; }
    public Guid CurrentUserId { get; init; }
} 