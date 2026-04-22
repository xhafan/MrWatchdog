using CoreBackend.Features.Jobs.Domain;
using CoreDdd.Nhibernate.Repositories;
using CoreDdd.Nhibernate.UnitOfWorks;
using CoreUtils;

namespace CoreBackend.Infrastructure.Repositories;

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