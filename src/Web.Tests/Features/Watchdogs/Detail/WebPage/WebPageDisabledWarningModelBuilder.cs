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

public class WebPageDisabledWarningModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private ICoreBus? _bus;
    private long _watchdogId;
    private long _watchdogWebPageId;

    public WebPageDisabledWarningModelBuilder WithBus(ICoreBus bus)
    {
        _bus = bus;
        return this;
    }
    
    public WebPageDisabledWarningModelBuilder WithWatchdogId(long watchdogId)
    {
        _watchdogId = watchdogId;
        return this;
    }       
    
    public WebPageDisabledWarningModelBuilder WithWatchdogWebPageId(long watchdogWebPageId)
    {
        _watchdogWebPageId = watchdogWebPageId;
        return this;
    }  
    
    public WebPageDisabledWarningModel Build()
    {
        _bus ??= A.Fake<ICoreBus>();
        
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetWatchdogWebPageDisabledWarningQueryHandler(
            unitOfWork,
            new NhibernateRepository<Watchdog>(unitOfWork)
        ));
        
        var model = new WebPageDisabledWarningModel(
            new QueryExecutor(queryHandlerFactory),
            _bus
        )
        {
            WatchdogId = _watchdogId,
            WatchdogWebPageId = _watchdogWebPageId
        };
        ModelValidator.ValidateModel(model);
        return model;
    }
}