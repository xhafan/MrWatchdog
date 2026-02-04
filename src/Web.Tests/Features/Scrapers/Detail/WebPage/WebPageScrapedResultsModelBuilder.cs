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

public class WebPageScrapedResultsModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private ICoreBus? _bus;
    private IAuthorizationService? _authorizationService;
    private long _scraperId;
    private long _scraperWebPageId;

    public WebPageScrapedResultsModelBuilder WithBus(ICoreBus bus)
    {
        _bus = bus;
        return this;
    }

    public WebPageScrapedResultsModelBuilder WithAuthorizationService(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
        return this;
    }
    
    public WebPageScrapedResultsModelBuilder WithScraperId(long scraperId)
    {
        _scraperId = scraperId;
        return this;
    }       
    
    public WebPageScrapedResultsModelBuilder WithScraperWebPageId(long scraperWebPageId)
    {
        _scraperWebPageId = scraperWebPageId;
        return this;
    } 

    public WebPageScrapedResultsModel Build()
    {
        _bus ??= A.Fake<ICoreBus>();

        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetScraperWebPageScrapedResultsQueryHandler(
            unitOfWork,
            new NhibernateRepository<Scraper>(unitOfWork)
        ));

        if (_authorizationService == null)
        {
            _authorizationService = A.Fake<IAuthorizationService>();
            A.CallTo(() => _authorizationService.AuthorizeAsync(A<ClaimsPrincipal>._, A<long>._, A<IAuthorizationRequirement[]>._))
                .Returns(AuthorizationResult.Success());
        }

        var model = new WebPageScrapedResultsModel(
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