using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HeroBoxAI.Domain.Repositories;
using MediatR;

namespace HeroBoxAI.Application.Clans.Queries.GetClanMembers;

public class GetClanMembersQueryHandler : IRequestHandler<GetClanMembersQuery, List<ClanMemberDto>>
{
    private readonly IClanRepository _clanRepository;

    public GetClanMembersQueryHandler(IClanRepository clanRepository)
    {
        _clanRepository = clanRepository;
    }

    public async Task<List<ClanMemberDto>> Handle(GetClanMembersQuery request, CancellationToken cancellationToken)
    {
        var clan = await _clanRepository.GetClanWithDetailsAsync(request.ClanId, cancellationToken);

        if (clan == null || clan.Members == null)
        {
            return new List<ClanMemberDto>();
        }

        return clan.Members.Select(member => new ClanMemberDto
        {
            Id = member.Id,
            Username = member.Username,
            Email = member.Email,
            Role = member.Role,
            Status = member.Status,
            JoinedAt = member.CreatedAt, // Using CreatedAt as join date for now
            HeroCount = member.Heroes?.Count ?? 0
        }).ToList();
    }
} 