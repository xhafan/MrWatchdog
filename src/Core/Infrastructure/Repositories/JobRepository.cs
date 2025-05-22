using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Jobs.Domain;

namespace MrWatchdog.Core.Infrastructure.Repositories;

public class JobRepository(NhibernateUnitOfWork unitOfWork) : NhibernateRepository<Job>(unitOfWork), IJobRepository
{
    public async Task<Job?> GetByGuidAsync(Guid guid)
    {
        return await Session.QueryOver<Job>()
            .Where(x => x.Guid == guid)
            .SingleOrDefaultAsync();
    }
}