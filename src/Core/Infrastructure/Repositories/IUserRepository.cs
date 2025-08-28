using MrWatchdog.Core.Features.Account.Domain;

namespace MrWatchdog.Core.Infrastructure.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}