using CoreDdd.Domain;
using CoreDdd.Nhibernate.Repositories;
using CoreDdd.Nhibernate.UnitOfWorks;

namespace MrWatchdog.TestsShared.Extensions;

// Copied from CoreDdd.Nhibernate.TestHelpers.NhibernateUnitOfWorkExtensions to support Entity Id of type long
public static class NhibernateUnitOfWorkExtensions
{
    public static void Save<TAggregateRoot>(this NhibernateUnitOfWork unitOfWork, TAggregateRoot aggregateRoot) 
        where TAggregateRoot : Entity<long>, IAggregateRoot
    {
        var repository = new NhibernateRepository<TAggregateRoot, long>(unitOfWork);
        repository.Save(aggregateRoot);
        unitOfWork.Flush();
    }
    
    public static TAggregateRoot? Get<TAggregateRoot>(this NhibernateUnitOfWork unitOfWork, long id) 
        where TAggregateRoot : Entity<long>, IAggregateRoot
    {
        var repository = new NhibernateRepository<TAggregateRoot, long>(unitOfWork);
        return repository.Get(id);
    }    

    public static TAggregateRoot LoadById<TAggregateRoot>(this NhibernateUnitOfWork unitOfWork, long id) 
        where TAggregateRoot : Entity<long>, IAggregateRoot
    {
        var repository = new NhibernateRepository<TAggregateRoot, long>(unitOfWork);
        return repository.LoadById(id);
    }  
}