using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;

namespace MrWatchdog.Core.Infrastructure.Repositories;

public class UserRepository(NhibernateUnitOfWork unitOfWork) : NhibernateRepository<User>(unitOfWork), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await Session.QueryOver<User>()
            .Where(x => x.Email == email)
            .SingleOrDefaultAsync();
    }
}