using System;
using System.Threading;
using System.Threading.Tasks;
using HeroBoxAI.Domain.Entities;
using HeroBoxAI.Domain.Repositories;
using HeroBoxAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HeroBoxAI.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(HeroBoxDbContext context) : base(context)
    {
    }

    public override Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.Include(u => u.Clan)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .Include(u => u.Clan)
            .Include(u => u.Heroes)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .Include(u => u.Clan)
            .Include(u => u.Heroes)
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AnyAsync(u => u.Username == username, cancellationToken);
    }
} 