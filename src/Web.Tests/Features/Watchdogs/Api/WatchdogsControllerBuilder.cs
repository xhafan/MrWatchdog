using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Watchdogs.Api;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Api;

public class WatchdogsControllerBuilder(NhibernateUnitOfWork unitOfWork)
{
    private ICoreBus? _bus;

    public WatchdogsControllerBuilder WithBus(ICoreBus bus)
    {
        _bus = bus;
        return this;
    }

    public WatchdogsController Build()
    {   
        _bus ??= A.Fake<ICoreBus>();

        var queryHandlerFactory = new FakeQueryHandlerFactory();

        var watchdogRepository = new NhibernateRepository<Watchdog>(unitOfWork);
        queryHandlerFactory.RegisterQueryHandler(new DoesWatchdogExitsQueryHandler(
            unitOfWork,
            watchdogRepository
        ));

        return new WatchdogsController(
            _bus,
            new QueryExecutor(queryHandlerFactory)
        );
    }
}