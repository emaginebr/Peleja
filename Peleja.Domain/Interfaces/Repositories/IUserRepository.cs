namespace Peleja.Domain.Interfaces.Repositories;

using Peleja.Domain.Models;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(long userId);
    Task<User?> GetByNauthUserIdAsync(long tenantId, string nauthUserId);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
}
