using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Watchdogs.Detail.Overview;
using Rebus.Bus;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.Overview;

public class OverviewModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private IBus? _bus;

    public OverviewModelBuilder WithBus(IBus bus)
    {
        _bus = bus;
        return this;
    }
    
    public OverviewModel Build()
    {
        _bus ??= A.Fake<IBus>();
        
        var model = new OverviewModel(
            new QueryExecutor(
                new FakeQueryHandlerFactory<GetWatchdogOverviewArgsQuery>(
                    new GetWatchdogOverviewArgsQueryHandler(
                        unitOfWork,
                        new NhibernateRepository<Watchdog>(unitOfWork)
                    )
                )
            ),
            _bus
        );
        ModelValidator.ValidateModel(model);
        return model;
    }
}