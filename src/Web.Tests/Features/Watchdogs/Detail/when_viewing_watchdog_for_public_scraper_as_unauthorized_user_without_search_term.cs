using System.Security.Claims;
using FakeItEasy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Scrapers;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.Search;
using MrWatchdog.Web.Infrastructure.Authorizations;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Search;

[TestFixture]
public class when_viewing_watchdog_search_for_public_scraper_as_unauthorized_user_without_search_term : BaseDatabaseTest
{
    private SearchModel _model = null!;
    private WatchdogSearch _watchdogSearch = null!;
    private Scraper _scraper = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();

        var authorizationService = A.Fake<IAuthorizationService>();
        A.CallTo(() => authorizationService.AuthorizeAsync(
                A<ClaimsPrincipal>._,
                _watchdogSearch.Id,
                A<IAuthorizationRequirement[]>.That.Matches(p => p.OfType<WatchdogSearchOwnerOrSuperAdminRequirement>().Any())
            ))
            .Returns(AuthorizationResult.Failed());
        
        _model = new SearchModelBuilder(UnitOfWork)
            .WithAuthorizationService(authorizationService)
            .Build();
        
        _actionResult = await _model.OnGet(_watchdogSearch.Id);
    }

    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<RedirectResult>();
        var redirectResult = (RedirectResult)_actionResult;
        redirectResult.Url.ShouldBe(ScraperUrlConstants.ScraperScrapingResultsUrlTemplate.WithScraperId(_scraper.Id));
    }

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork)
            .WithName("scraper name")
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .Build();
        var scraperWebPage = _scraper.WebPages.Single();
        _scraper.SetScrapingResults(scraperWebPage.Id, ["<div>text 1</div>"]);
        _scraper.EnableWebPage(scraperWebPage.Id);
        _scraper.MakePublic();
        
        _watchdogSearch = new WatchdogSearchBuilder(UnitOfWork)
            .WithScraper(_scraper)
            .WithSearchTerm(null)
            .Build();

        UnitOfWork.Flush();
    }
}