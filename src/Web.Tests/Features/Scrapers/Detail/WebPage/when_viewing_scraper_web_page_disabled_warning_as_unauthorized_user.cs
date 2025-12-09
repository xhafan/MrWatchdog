using System.Security.Claims;
using FakeItEasy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.Detail.WebPage;
using MrWatchdog.Web.Infrastructure.Authorizations;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Detail.WebPage;

[TestFixture]
public class when_viewing_scraper_web_page_disabled_warning_as_unauthorized_user : BaseDatabaseTest
{
    private WebPageDisabledWarningModel _model = null!;
    private Scraper _scraper = null!;
    private long _scraperWebPageId;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        var authorizationService = A.Fake<IAuthorizationService>();
        A.CallTo(() => authorizationService.AuthorizeAsync(
                A<ClaimsPrincipal>._,
                _scraper.Id,
                A<IAuthorizationRequirement[]>.That.Matches(p => p.OfType<ScraperOwnerOrSuperAdminRequirement>().Any())
            ))
            .Returns(AuthorizationResult.Failed());

        _model = new WebPageDisabledWarningModelBuilder(UnitOfWork)
            .WithAuthorizationService(authorizationService)
            .WithScraperId(_scraper.Id)
            .WithScraperWebPageId(_scraperWebPageId)
            .Build();

        _actionResult = await _model.OnGet();
    }

    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<ForbidResult>();
    }  

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork)
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                SelectText = true,
                Name = "url.com/page"
            })
            .Build();
        _scraperWebPageId = _scraper.WebPages.Single().Id;
        _scraper.SetScrapingResults(_scraperWebPageId, ["Another World", "Doom 1"]);
        _scraper.EnableWebPage(_scraperWebPageId);

        UnitOfWork.Flush();
    }    
}