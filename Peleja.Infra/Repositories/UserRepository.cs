namespace Peleja.Infra.Repositories;

using Microsoft.EntityFrameworkCore;
using Peleja.Domain.Models;
using Peleja.Infra.Context;
using Peleja.Infra.Interfaces.Repositories;

public class UserRepository : IUserRepository
{
    private readonly PelejaContext _context;

    public UserRepository(PelejaContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(long userId)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<User?> GetByNauthUserIdAsync(long tenantId, string nauthUserId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.NauthUserId == nauthUserId);
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }
}
