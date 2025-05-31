using System.Threading;
using System.Threading.Tasks;
using HeroBoxAI.Application.Common.Exceptions;
using HeroBoxAI.Domain.Repositories;
using MediatR;

namespace HeroBoxAI.Application.Clans.Commands.DeleteClan;

public class DeleteClanCommandHandler : IRequestHandler<DeleteClanCommand, bool>
{
    private readonly IClanRepository _clanRepository;

    public DeleteClanCommandHandler(IClanRepository clanRepository)
    {
        _clanRepository = clanRepository;
    }

    public async Task<bool> Handle(DeleteClanCommand request, CancellationToken cancellationToken)
    {
        // Get the existing clan with all details
        var clan = await _clanRepository.GetClanWithDetailsAsync(request.Id, cancellationToken);
        
        if (clan == null)
        {
            return false;
        }

        // Check if the current user is the clan founder
        if (clan.FounderId != request.CurrentUserId)
        {
            throw new ForbiddenException("Only the clan founder can delete the clan.");
        }

        // First, remove all members from the clan (set their ClanId to null)
        if (clan.Members != null)
        {
            foreach (var member in clan.Members)
            {
                member.ClanId = null;
            }
        }

        // Delete the clan - this should work now since we've handled the member relationships
        await _clanRepository.DeleteAsync(clan, cancellationToken);
        await _clanRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
} 