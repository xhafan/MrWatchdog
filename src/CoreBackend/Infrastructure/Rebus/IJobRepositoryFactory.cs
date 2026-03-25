using CoreBackend.Infrastructure.Repositories;

namespace CoreBackend.Infrastructure.Rebus;

public interface IJobRepositoryFactory
{
    IJobRepository Create();
}