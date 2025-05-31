using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HeroBoxAI.Domain.Entities;
using HeroBoxAI.Domain.Repositories;
using HeroBoxAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HeroBoxAI.Infrastructure.Repositories;

public class ClanRepository : Repository<Clan>, IClanRepository
{
    public ClanRepository(HeroBoxDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Clan> GetClanWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Clans
            .Include(c => c.Founder)
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Clan>> GetAllClansWithFounderAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Clans
            .Include(c => c.Founder)
            .ToListAsync(cancellationToken);
    }
} 