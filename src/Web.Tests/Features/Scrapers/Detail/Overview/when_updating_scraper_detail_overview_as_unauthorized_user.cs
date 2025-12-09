using System.Security.Claims;
using FakeItEasy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.Detail.Overview;
using MrWatchdog.Web.Infrastructure.Authorizations;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Detail.Overview;

[TestFixture]
public class when_updating_scraper_detail_overview_as_unauthorized_user : BaseDatabaseTest
{
    private IActionResult _actionResult = null!;
    private OverviewModel _model = null!;
    private ICoreBus _bus = null!;    
    private Scraper _scraper = null!;

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

        _model = new OverviewModelBuilder(UnitOfWork)
            .WithAuthorizationService(authorizationService)
            .WithBus(_bus)
            .Build();

        _model.ScraperOverviewArgs = new ScraperOverviewArgs
        {
            ScraperId = _scraper.Id,
            Name = "scraper updated name",
            Description = null,
            ScrapingIntervalInSeconds = 60,
            IntervalBetweenSameResultNotificationsInDays = 30,
            NumberOfFailedScrapingAttemptsBeforeAlerting = 5
        };

        _actionResult = await _model.OnPost();
    }

    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<ForbidResult>();
    }

    [Test]
    public void command_not_is_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(new UpdateScraperOverviewCommand(_model.ScraperOverviewArgs)))
            .MustNotHaveHappened();
    }

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork).Build();
    }    
}