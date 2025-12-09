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
public class when_removing_scraper_web_page_as_unauthorized_user : BaseDatabaseTest
{
    private WebPageModel _model = null!;
    private Scraper _scraper = null!;
    private ICoreBus _bus = null!;
    private IActionResult _actionResult = null!;
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

        _model = new WebPageModelBuilder(UnitOfWork)
            .WithBus(_bus)
            .WithAuthorizationService(authorizationService)
            .Build();
        
        _actionResult = await _model.OnPostRemoveScraperWebPage(_scraper.Id, _scraperWebPageId);
    }

    [Test]
    public void command_is_not_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(new RemoveScraperWebPageCommand(_scraper.Id, _scraperWebPageId)))
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