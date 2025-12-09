using System.Security.Claims;
using FakeItEasy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.Detail.WebPage;
using MrWatchdog.Web.Infrastructure.Authorizations;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Detail.WebPage;

[TestFixture]
public class when_updating_scraper_web_page_as_unauthorized_user : BaseDatabaseTest
{
    private IActionResult _actionResult = null!;
    private WebPageOverviewModel _model = null!;
    private ICoreBus _bus = null!;    
    private Scraper _scraper = null!;
    private long _scraperWebPageId;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        _bus = A.Fake<ICoreBus>();

        var authorizationService = A.Fake<IAuthorizationService>();
        A.CallTo(() => authorizationService.AuthorizeAsync(
                A<ClaimsPrincipal>._,
                _scraper.Id,
                A<IAuthorizationRequirement[]>.That.Matches(p => p.OfType<ScraperOwnerOrSuperAdminRequirement>().Any())
            ))
            .Returns(AuthorizationResult.Failed());

        _model = new WebPageOverviewModelBuilder(UnitOfWork)
            .WithBus(_bus)
            .WithAuthorizationService(authorizationService)
            .WithScraperWebPageArgs(new ScraperWebPageArgs
            {
                ScraperId = _scraper.Id,
                ScraperWebPageId = _scraperWebPageId,
                Url = "http://url.com/page2",
                Selector = ".selector2",
                Name = "url.com/page2"
            })
            .Build();

        _actionResult = await _model.OnPost();
    }

    [Test]
    public void command_is_not_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(new UpdateScraperWebPageCommand(_model.ScraperWebPageArgs)))
            .MustNotHaveHappened();
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
                Name = "url.com/page"
            })
            .Build();
        _scraperWebPageId = _scraper.WebPages.Single().Id;
    }    
}