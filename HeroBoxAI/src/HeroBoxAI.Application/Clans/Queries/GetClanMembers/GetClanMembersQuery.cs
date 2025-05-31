using System;
using System.Collections.Generic;
using MediatR;

namespace HeroBoxAI.Application.Clans.Queries.GetClanMembers;

public record GetClanMembersQuery(Guid ClanId) : IRequest<List<ClanMemberDto>>; 