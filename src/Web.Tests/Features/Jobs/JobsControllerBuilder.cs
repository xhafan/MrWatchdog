using CoreBackend.Features.Jobs.Queries;
using CoreBackend.Infrastructure.Repositories;
using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using CoreWeb.Features.Jobs;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Infrastructure.Rebus;

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
            jobRepository,
            new MessageTypeGetter(RebusAssembliesHelper.GetAssembliesWithTypesDerivedFromBaseMessage)
        );
    }
}