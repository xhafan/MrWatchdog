using CoreDdd.Nhibernate.UnitOfWorks;
using CoreUtils;
using MrWatchdog.Core.Features.Jobs.Domain;

namespace MrWatchdog.Core.Infrastructure.Repositories;

public class JobRepository(NhibernateUnitOfWork unitOfWork) : NhibernateRepository<Job>(unitOfWork), IJobRepository
{
    public async Task<Job?> GetByGuidAsync(Guid jobGuid)
    {
        return await Session.QueryOver<Job>()
            .Where(x => x.Guid == jobGuid)
            .SingleOrDefaultAsync();
    }
    
    public async Task<Job> LoadByGuidAsync(Guid jobGuid)
    {
        var job = await GetByGuidAsync(jobGuid);
        Guard.Hope(job != null, $"Job {jobGuid} not found.");
        return job;
    }    
}