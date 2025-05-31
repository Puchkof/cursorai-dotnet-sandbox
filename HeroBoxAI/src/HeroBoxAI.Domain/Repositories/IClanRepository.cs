using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HeroBoxAI.Domain.Entities;

namespace HeroBoxAI.Domain.Repositories;

/// <summary>
/// Repository interface for Clan entity operations
/// </summary>
public interface IClanRepository : IRepository<Clan>
{
    /// <summary>
    /// Gets a clan by ID with founder and members information
    /// </summary>
    Task<Clan> GetClanWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all clans with founder information
    /// </summary>
    Task<IEnumerable<Clan>> GetAllClansWithFounderAsync(CancellationToken cancellationToken = default);
} 