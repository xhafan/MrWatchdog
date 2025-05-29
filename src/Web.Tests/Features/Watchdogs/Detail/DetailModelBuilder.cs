using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Watchdogs.Detail;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail;

public class DetailModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    public DetailModel Build()
    {
        var model = new DetailModel(
            new QueryExecutor(
                new FakeQueryHandlerFactory<GetWatchdogArgsQuery>(
                    new GetWatchdogArgsQueryHandler(
                        unitOfWork,
                        new NhibernateRepository<Watchdog>(unitOfWork)
                    )
                )
            )
        );
        ModelValidator.ValidateModel(model);
        return model;
    }
}