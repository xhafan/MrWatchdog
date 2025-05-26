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
        return new JobsController(
            new QueryExecutor(
                new FakeQueryHandlerFactory<GetJobQuery>(
                    new GetJobQueryHandler(
                        unitOfWork,
                        new JobRepository(unitOfWork)
                    )
                )
            )
        );
    }
}