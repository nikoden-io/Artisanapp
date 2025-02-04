using Artisaback.Domain.Entities;

namespace Artisaback.Domain.IRepositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);

    Task CreateAsync(User user);
    Task UpdateAsync(User user);
}