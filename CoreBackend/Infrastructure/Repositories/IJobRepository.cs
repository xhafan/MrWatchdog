using CoreBackend.Features.Jobs.Domain;
using CoreDdd.Domain.Repositories;

namespace CoreBackend.Infrastructure.Repositories;

public interface IJobRepository : IRepository<Job>
{
    Task<Job?> GetByGuidAsync(Guid jobGuid);
    Task<Job> LoadByGuidAsync(Guid jobGuid);
}