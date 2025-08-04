using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Watchdogs.Alert.Overview;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Alert.Overview;

public class OverviewModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private ICoreBus? _bus;
    private WatchdogAlertOverviewArgs? _watchdogAlertOverviewArgs;

    public OverviewModelBuilder WithBus(ICoreBus bus)
    {
        _bus = bus;
        return this;
    }

    public OverviewModelBuilder WithWatchdogAlertOverviewArgs(WatchdogAlertOverviewArgs watchdogAlertOverviewArgs)
    {
        _watchdogAlertOverviewArgs = watchdogAlertOverviewArgs;
        return this;
    }
    
    public OverviewModel Build()
    {
        _bus ??= A.Fake<ICoreBus>();
        
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetWatchdogAlertOverviewArgsQueryHandler(
            unitOfWork,
            new NhibernateRepository<WatchdogAlert>(unitOfWork)
        ));
        
        var model = new OverviewModel(
            new QueryExecutor(queryHandlerFactory),
            _bus
        )
        {
            WatchdogAlertOverviewArgs = _watchdogAlertOverviewArgs ?? new WatchdogAlertOverviewArgs
            {
                WatchdogAlertId = 0,
                SearchTerm = null
            }
        };
        ModelValidator.ValidateModel(model);
        return model;
    }
}