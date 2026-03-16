using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.Core.Infrastructure.Rebus;

public interface IJobRepositoryFactory
{
    IJobRepository Create();
    void Release(IJobRepository unitOfWork);
}