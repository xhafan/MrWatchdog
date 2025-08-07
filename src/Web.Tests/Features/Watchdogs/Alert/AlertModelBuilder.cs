using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Watchdogs.Alert;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Alert;

public class AlertModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private ICoreBus? _bus;
    
    public AlertModelBuilder WithBus(ICoreBus bus)
    {
        _bus = bus;
        return this;
    }  
    
    public AlertModel Build()
    {
        _bus ??= A.Fake<ICoreBus>();
        
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetWatchdogAlertArgsQueryHandler(
            unitOfWork,
            new NhibernateRepository<WatchdogAlert>(unitOfWork)
        ));
        
        queryHandlerFactory.RegisterQueryHandler(new GetWatchdogScrapingResultsArgsQueryHandler(
            unitOfWork,
            new NhibernateRepository<Watchdog>(unitOfWork)
        ));

        var model = new AlertModel(
            new QueryExecutor(queryHandlerFactory),
            _bus
        );
        ModelValidator.ValidateModel(model);
        return model;
    }
}