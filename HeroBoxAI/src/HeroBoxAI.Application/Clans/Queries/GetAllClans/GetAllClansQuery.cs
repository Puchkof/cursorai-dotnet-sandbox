using System.Collections.Generic;
using MediatR;

namespace HeroBoxAI.Application.Clans.Queries.GetAllClans;

public record GetAllClansQuery : IRequest<List<ClanDto>>; 