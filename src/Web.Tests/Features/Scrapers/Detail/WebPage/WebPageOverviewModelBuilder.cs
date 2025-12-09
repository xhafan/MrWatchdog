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

public class WebPageOverviewModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private ICoreBus? _bus;
    private IAuthorizationService? _authorizationService;
    private ScraperWebPageArgs? _scraperWebPageArgs;

    public WebPageOverviewModelBuilder WithBus(ICoreBus bus)
    {
        _bus = bus;
        return this;
    }
    
    public WebPageOverviewModelBuilder WithAuthorizationService(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
        return this;
    }

    public WebPageOverviewModelBuilder WithScraperWebPageArgs(ScraperWebPageArgs scraperWebPageArgs)
    {
        _scraperWebPageArgs = scraperWebPageArgs;
        return this;
    }       
    
    public WebPageOverviewModel Build()
    {
        _bus ??= A.Fake<ICoreBus>();
        
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetScraperWebPageArgsQueryHandler(
            unitOfWork,
            new NhibernateRepository<Scraper>(unitOfWork)
        ));

        if (_authorizationService == null)
        {
            _authorizationService = A.Fake<IAuthorizationService>();
            A.CallTo(() => _authorizationService.AuthorizeAsync(A<ClaimsPrincipal>._, A<long>._, A<IAuthorizationRequirement[]>._))
                .Returns(AuthorizationResult.Success());
        }

        var model = new WebPageOverviewModel(
            new QueryExecutor(queryHandlerFactory),
            _bus,
            _authorizationService
        )
        {
            ScraperWebPageArgs = _scraperWebPageArgs!
        };
        ModelValidator.ValidateModel(model);
        return model;
    }
}