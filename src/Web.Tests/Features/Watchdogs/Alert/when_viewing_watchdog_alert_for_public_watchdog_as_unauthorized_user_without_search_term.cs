using System.Security.Claims;
using FakeItEasy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Alert;
using MrWatchdog.Web.Infrastructure.Authorizations;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Alert;

[TestFixture]
public class when_viewing_watchdog_alert_for_public_watchdog_as_unauthorized_user_without_search_term : BaseDatabaseTest
{
    private AlertModel _model = null!;
    private WatchdogAlert _watchdogAlert = null!;
    private Watchdog _watchdog = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();

        var authorizationService = A.Fake<IAuthorizationService>();
        A.CallTo(() => authorizationService.AuthorizeAsync(
                A<ClaimsPrincipal>._,
                _watchdogAlert.Id,
                A<IAuthorizationRequirement[]>.That.Matches(p => p.OfType<WatchdogAlertOwnerOrSuperAdminRequirement>().Any())
            ))
            .Returns(AuthorizationResult.Failed());
        
        _model = new AlertModelBuilder(UnitOfWork)
            .WithAuthorizationService(authorizationService)
            .Build();
        
        _actionResult = await _model.OnGet(_watchdogAlert.Id);
    }

    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<RedirectResult>();
        var redirectResult = (RedirectResult)_actionResult;
        redirectResult.Url.ShouldBe(WatchdogUrlConstants.WatchdogScrapingResultsUrlTemplate.WithWatchdogId(_watchdog.Id));
    }

    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithName("watchdog name")
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .Build();
        var watchdogWebPage = _watchdog.WebPages.Single();
        _watchdog.SetScrapingResults(watchdogWebPage.Id, ["<div>text 1</div>"]);
        _watchdog.EnableWebPage(watchdogWebPage.Id);
        _watchdog.MakePublic();
        
        _watchdogAlert = new WatchdogAlertBuilder(UnitOfWork)
            .WithWatchdog(_watchdog)
            .WithSearchTerm(null)
            .Build();

        UnitOfWork.Flush();
    }
}