using System.Threading;
using System.Threading.Tasks;
using HeroBoxAI.Domain.Repositories;
using MediatR;

namespace HeroBoxAI.Application.Clans.Queries.GetClanById;

public class GetClanByIdQueryHandler : IRequestHandler<GetClanByIdQuery, ClanDto>
{
    private readonly IClanRepository _clanRepository;

    public GetClanByIdQueryHandler(IClanRepository clanRepository)
    {
        _clanRepository = clanRepository;
    }

    public async Task<ClanDto> Handle(GetClanByIdQuery request, CancellationToken cancellationToken)
    {
        var clan = await _clanRepository.GetClanWithDetailsAsync(request.Id, cancellationToken);

        if (clan == null)
        {
            return null;
        }

        return new ClanDto
        {
            Id = clan.Id,
            Name = clan.Name,
            Tag = clan.Tag,
            Description = clan.Description,
            Level = clan.Level,
            FounderId = clan.FounderId,
            FounderName = clan.Founder?.Username ?? "Unknown",
            MemberCount = clan.Members?.Count ?? 0,
            CreatedAt = clan.CreatedAt
        };
    }
} 