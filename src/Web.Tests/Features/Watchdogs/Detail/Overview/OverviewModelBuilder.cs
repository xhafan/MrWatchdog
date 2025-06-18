using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Watchdogs.Detail.Overview;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.Overview;

public class OverviewModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private ICoreBus? _bus;

    public OverviewModelBuilder WithBus(ICoreBus bus)
    {
        _bus = bus;
        return this;
    }
    
    public OverviewModel Build()
    {
        _bus ??= A.Fake<ICoreBus>();
        
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetWatchdogOverviewArgsQueryHandler(
            unitOfWork,
            new NhibernateRepository<Watchdog>(unitOfWork)
        ));
        
        var model = new OverviewModel(
            new QueryExecutor(queryHandlerFactory),
            _bus
        );
        ModelValidator.ValidateModel(model);
        return model;
    }
}