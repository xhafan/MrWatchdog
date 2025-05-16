using CoreDdd.Domain;
using CoreDdd.Domain.Repositories;

namespace MrWatchdog.Core.Infrastructure.Repositories;

public interface IRepository<TAggregateRoot> : IRepository<TAggregateRoot, long> 
    where TAggregateRoot : Entity<long>, IAggregateRoot;