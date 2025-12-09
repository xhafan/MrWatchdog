using System.Security.Claims;
using FakeItEasy;
using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Web.Features.Scrapers.Detail.WebPage;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Detail.WebPage;

public class WebPageTurboFrameModelBuilder
{
    private IAuthorizationService? _authorizationService;
    private long _scraperId;
    private long _scraperWebPageId;

    public WebPageTurboFrameModelBuilder WithAuthorizationService(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
        return this;
    }
    
    public WebPageTurboFrameModelBuilder WithScraperId(long scraper)
    {
        _scraperId = scraper;
        return this;
    }       
    
    public WebPageTurboFrameModelBuilder WithScraperWebPageId(long scraperWebPageId)
    {
        _scraperWebPageId = scraperWebPageId;
        return this;
    } 

    public WebPageTurboFrameModel Build()
    {
        if (_authorizationService == null)
        {
            _authorizationService = A.Fake<IAuthorizationService>();
            A.CallTo(() => _authorizationService.AuthorizeAsync(A<ClaimsPrincipal>._, A<long>._, A<IAuthorizationRequirement[]>._))
                .Returns(AuthorizationResult.Success());
        }

        var model = new WebPageTurboFrameModel(
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