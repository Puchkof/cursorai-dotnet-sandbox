using System;
using System.Threading;
using System.Threading.Tasks;
using HeroBoxAI.Domain.Entities;
using HeroBoxAI.Domain.Repositories;
using MediatR;

namespace HeroBoxAI.Application.Clans.Commands.CreateClan;

public class CreateClanCommandHandler : IRequestHandler<CreateClanCommand, ClanDto>
{
    private readonly IClanRepository _clanRepository;
    private readonly IRepository<User> _userRepository;

    public CreateClanCommandHandler(IClanRepository clanRepository, IRepository<User> userRepository)
    {
        _clanRepository = clanRepository;
        _userRepository = userRepository;
    }

    public async Task<ClanDto> Handle(CreateClanCommand request, CancellationToken cancellationToken)
    {
        // Validate founder exists
        var founder = await _userRepository.GetByIdAsync(request.FounderId, cancellationToken);
        if (founder == null)
        {
            return null;
        }

        // Create new clan
        var clan = new Clan
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Tag = request.Tag,
            Description = request.Description,
            FounderId = request.FounderId,
            Level = 1,
            CreatedAt = DateTime.UtcNow
        };

        await _clanRepository.AddAsync(clan, cancellationToken);
        await _clanRepository.SaveChangesAsync(cancellationToken);

        // Get the full clan with founder details
        var createdClan = await _clanRepository.GetClanWithDetailsAsync(clan.Id, cancellationToken);

        // Map to DTO
        return new ClanDto
        {
            Id = createdClan.Id,
            Name = createdClan.Name,
            Tag = createdClan.Tag,
            Description = createdClan.Description,
            Level = createdClan.Level,
            FounderId = createdClan.FounderId,
            FounderName = createdClan.Founder?.Username ?? "Unknown",
            MemberCount = 1, // Initially just the founder
            CreatedAt = createdClan.CreatedAt
        };
    }
} 