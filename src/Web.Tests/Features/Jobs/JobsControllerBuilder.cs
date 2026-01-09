using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using MrWatchdog.Core.Features.Jobs.Queries;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Jobs;

namespace MrWatchdog.Web.Tests.Features.Jobs;

public class JobsControllerBuilder(NhibernateUnitOfWork unitOfWork)
{
    public JobsController Build()
    {
        var queryHandlerFactory = new FakeQueryHandlerFactory();

        var jobRepository = new JobRepository(unitOfWork);
        queryHandlerFactory.RegisterQueryHandler(new GetJobQueryHandler(
            unitOfWork,
            jobRepository
        ));
        queryHandlerFactory.RegisterQueryHandler(new GetRelatedDomainEventJobQueryHandler(
            unitOfWork
        ));        
        
        return new JobsController(
            new QueryExecutor(queryHandlerFactory),
            jobRepository
        );
    }
}