using MrWatchdog.Core.Features.Jobs.Domain;

namespace MrWatchdog.Core.Infrastructure.Repositories;

public interface IJobRepository : IRepository<Job>
{
    Task<Job?> GetByGuidAsync(Guid jobGuid);
    Task<Job> LoadByGuidAsync(Guid jobGuid);
}