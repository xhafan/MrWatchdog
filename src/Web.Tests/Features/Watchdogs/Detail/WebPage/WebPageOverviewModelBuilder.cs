using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.WebPage;

public class WebPageOverviewModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private ICoreBus? _bus;
    private WatchdogWebPageArgs? _watchdogWebPageArgs;

    public WebPageOverviewModelBuilder WithBus(ICoreBus bus)
    {
        _bus = bus;
        return this;
    }
    
    public WebPageOverviewModelBuilder WithWatchdogWebPageArgs(WatchdogWebPageArgs watchdogWebPageArgs)
    {
        _watchdogWebPageArgs = watchdogWebPageArgs;
        return this;
    }       
    
    public WebPageOverviewModel Build()
    {
        _bus ??= A.Fake<ICoreBus>();
        
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetWatchdogWebPageArgsQueryHandler(
            unitOfWork,
            new NhibernateRepository<Watchdog>(unitOfWork)
        ));
        
        var model = new WebPageOverviewModel(
            new QueryExecutor(queryHandlerFactory),
            _bus
        )
        {
            WatchdogWebPageArgs = _watchdogWebPageArgs!
        };
        ModelValidator.ValidateModel(model);
        return model;
    }
}