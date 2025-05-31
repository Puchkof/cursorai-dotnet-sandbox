using System.Threading;
using System.Threading.Tasks;
using HeroBoxAI.Application.Common.Exceptions;
using HeroBoxAI.Domain.Repositories;
using MediatR;

namespace HeroBoxAI.Application.Clans.Commands.UpdateClan;

public class UpdateClanCommandHandler : IRequestHandler<UpdateClanCommand, ClanDto>
{
    private readonly IClanRepository _clanRepository;

    public UpdateClanCommandHandler(IClanRepository clanRepository)
    {
        _clanRepository = clanRepository;
    }

    public async Task<ClanDto> Handle(UpdateClanCommand request, CancellationToken cancellationToken)
    {
        // Validate input parameters
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Clan name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Tag))
        {
            throw new ValidationException("Clan tag is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new ValidationException("Clan description is required.");
        }

        // Get the existing clan
        var clan = await _clanRepository.GetClanWithDetailsAsync(request.Id, cancellationToken);
        
        if (clan == null)
        {
            return null;
        }

        // Check if the current user is the clan founder
        if (clan.FounderId != request.CurrentUserId)
        {
            throw new ForbiddenException("Only the clan founder can update the clan.");
        }

        // Update clan properties - EF Core will track these changes automatically
        clan.Name = request.Name;
        clan.Tag = request.Tag;
        clan.Description = request.Description;

        // Save changes to persist the updates
        await _clanRepository.SaveChangesAsync(cancellationToken);

        // Return the updated clan DTO
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