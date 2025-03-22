using System;
using MediatR;

namespace HeroBoxAI.Application.Clans.Queries.GetClanById;

public record GetClanByIdQuery(Guid Id) : IRequest<ClanDto>; 