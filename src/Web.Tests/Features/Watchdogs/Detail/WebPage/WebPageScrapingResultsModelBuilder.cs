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

public class WebPageScrapingResultsModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private ICoreBus? _bus;
    private IAuthorizationService? _authorizationService;
    private long _watchdogId;
    private long _watchdogWebPageId;

    public WebPageScrapingResultsModelBuilder WithBus(ICoreBus bus)
    {
        _bus = bus;
        return this;
    }

    public WebPageScrapingResultsModelBuilder WithAuthorizationService(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
        return this;
    }
    
    public WebPageScrapingResultsModelBuilder WithWatchdogId(long watchdogId)
    {
        _watchdogId = watchdogId;
        return this;
    }       
    
    public WebPageScrapingResultsModelBuilder WithWatchdogWebPageId(long watchdogWebPageId)
    {
        _watchdogWebPageId = watchdogWebPageId;
        return this;
    } 

    public WebPageScrapingResultsModel Build()
    {
        _bus ??= A.Fake<ICoreBus>();

        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetWatchdogWebPageScrapingResultsQueryHandler(
            unitOfWork,
            new NhibernateRepository<Watchdog>(unitOfWork)
        ));

        if (_authorizationService == null)
        {
            _authorizationService = A.Fake<IAuthorizationService>();
            A.CallTo(() => _authorizationService.AuthorizeAsync(A<ClaimsPrincipal>._, A<long>._, A<IAuthorizationRequirement[]>._))
                .Returns(AuthorizationResult.Success());
        }

        var model = new WebPageScrapingResultsModel(
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