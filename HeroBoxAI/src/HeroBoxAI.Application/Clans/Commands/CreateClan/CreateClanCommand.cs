using System;
using MediatR;

namespace HeroBoxAI.Application.Clans.Commands.CreateClan;

public record CreateClanCommand : IRequest<ClanDto>
{
    public string Name { get; init; }
    public string Tag { get; init; }
    public string Description { get; init; }
    public Guid FounderId { get; init; }
} 