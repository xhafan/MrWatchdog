using System.Security.Claims;
using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Scrapers.Detail.WebPage;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Detail.WebPage;

public class WebPageDisabledWarningModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private ICoreBus? _bus;
    private IAuthorizationService? _authorizationService;

    private long _scraperId;
    private long _scraperWebPageId;

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
    
    public WebPageDisabledWarningModelBuilder WithScraperId(long scraper)
    {
        _scraperId = scraper;
        return this;
    }       
    
    public WebPageDisabledWarningModelBuilder WithScraperWebPageId(long scraperWebPageId)
    {
        _scraperWebPageId = scraperWebPageId;
        return this;
    }  
    
    public WebPageDisabledWarningModel Build()
    {
        _bus ??= A.Fake<ICoreBus>();
        
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetScraperWebPageDisabledWarningQueryHandler(
            unitOfWork,
            new NhibernateRepository<Scraper>(unitOfWork)
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
            ScraperId = _scraperId,
            ScraperWebPageId = _scraperWebPageId
        };
        ModelValidator.ValidateModel(model);
        return model;
    }
}