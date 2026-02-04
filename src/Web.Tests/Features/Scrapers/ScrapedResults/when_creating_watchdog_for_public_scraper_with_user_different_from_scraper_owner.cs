using System.Security.Claims;
using FakeItEasy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.ScrapingResults;
using MrWatchdog.Web.Infrastructure.Authorizations;

namespace MrWatchdog.Web.Tests.Features.Scrapers.ScrapingResults;

[TestFixture]
public class when_creating_watchdog_for_public_scraper_with_user_different_from_scraper_owner : BaseDatabaseTest
{
    private ScrapingResultsModel _model = null!;
    private Scraper _scraper = null!;
    private ICoreBus _bus = null!;
    private IActionResult _actionResult = null!;
    private User _actingUser = null!;

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

        _model = new ScrapingResultsModelBuilder(UnitOfWork)
            .WithBus(_bus)
            .WithAuthorizationService(authorizationService)
            .WithActingUser(_actingUser)
            .WithSearchTerm(" search term ")
            .Build();
        
        _actionResult = await _model.OnPostCreateWatchdog(_scraper.Id);
    }

    [Test]
    public void command_is_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(new CreateWatchdogCommand(_scraper.Id, "search term"))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<OkObjectResult>();
        var okObjectResult = (OkObjectResult) _actionResult;
        var value = okObjectResult.Value;
        value.ShouldBeOfType<string>();
        var jobGuid = (string) value;
        jobGuid.ShouldMatch(@"[0-9A-Fa-f\-]{36}");
    }

    private void _BuildEntities()
    {
        var user = new UserBuilder(UnitOfWork).Build();

        _scraper = new ScraperBuilder(UnitOfWork)
            .WithUser(user)
            .Build();
        _scraper.MakePublic();

        _actingUser = new UserBuilder(UnitOfWork).Build();

        UnitOfWork.Flush();
    }    
}