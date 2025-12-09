using System.Security.Claims;
using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using Microsoft.AspNetCore.Authorization;
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
    private IAuthorizationService? _authorizationService;

    private long _watchdogId;
    private long _watchdogWebPageId;

    public WebPageDisabledWarningModelBuilder WithBus(ICoreBus bus)
    {
        _bus = bus;
        return this;
    }

    public WebPageDisabledWarningModelBuilder WithAuthorizationService(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
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
        
        if (_authorizationService == null)
        {
            _authorizationService = A.Fake<IAuthorizationService>();
            A.CallTo(() => _authorizationService.AuthorizeAsync(A<ClaimsPrincipal>._, A<long>._, A<IAuthorizationRequirement[]>._))
                .Returns(AuthorizationResult.Success());
        }

        var model = new WebPageDisabledWarningModel(
            new QueryExecutor(queryHandlerFactory),
            _bus,
            _authorizationService
        )
        {
            WatchdogId = _watchdogId,
            WatchdogWebPageId = _watchdogWebPageId
        };
        ModelValidator.ValidateModel(model);
        return model;
    }
}