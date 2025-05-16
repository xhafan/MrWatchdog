using CoreDdd.Domain;
using CoreDdd.Nhibernate.Repositories;
using CoreDdd.Nhibernate.UnitOfWorks;

namespace MrWatchdog.Core.Infrastructure.Repositories;

public class NhibernateRepository<TAggregateRoot>(NhibernateUnitOfWork unitOfWork)
    : NhibernateRepository<TAggregateRoot, long>(unitOfWork), IRepository<TAggregateRoot>
    where TAggregateRoot : Entity<long>, IAggregateRoot;