using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HeroBoxAI.Domain.Repositories;
using MediatR;

namespace HeroBoxAI.Application.Clans.Queries.GetAllClans;

public class GetAllClansQueryHandler : IRequestHandler<GetAllClansQuery, List<ClanDto>>
{
    private readonly IClanRepository _clanRepository;

    public GetAllClansQueryHandler(IClanRepository clanRepository)
    {
        _clanRepository = clanRepository;
    }

    public async Task<List<ClanDto>> Handle(GetAllClansQuery request, CancellationToken cancellationToken)
    {
        var clans = await _clanRepository.GetAllClansWithFounderAsync(cancellationToken);
        
        return clans.Select(c => new ClanDto
        {
            Id = c.Id,
            Name = c.Name,
            Tag = c.Tag,
            Description = c.Description,
            Level = c.Level,
            FounderId = c.FounderId,
            FounderName = c.Founder?.Username ?? "Unknown",
            MemberCount = c.Members?.Count ?? 0,
            CreatedAt = c.CreatedAt
        }).ToList();
    }
} 