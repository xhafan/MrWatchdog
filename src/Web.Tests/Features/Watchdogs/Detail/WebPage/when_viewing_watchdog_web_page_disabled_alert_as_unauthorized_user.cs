using FakeItEasy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;
using MrWatchdog.Web.Infrastructure.Authorizations;
using System.Security.Claims;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.WebPage;

[TestFixture]
public class when_viewing_watchdog_web_page_disabled_alert_as_unauthorized_user : BaseDatabaseTest
{
    private WebPageDisabledWarningModel _model = null!;
    private Watchdog _watchdog = null!;
    private long _watchdogWebPageId;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        var authorizationService = A.Fake<IAuthorizationService>();
        A.CallTo(() => authorizationService.AuthorizeAsync(
                A<ClaimsPrincipal>._,
                _watchdog.Id,
                A<IAuthorizationRequirement[]>.That.Matches(p => p.OfType<WatchdogOwnerOrSuperAdminRequirement>().Any())
            ))
            .Returns(AuthorizationResult.Failed());

        _model = new WebPageDisabledWarningModelBuilder(UnitOfWork)
            .WithAuthorizationService(authorizationService)
            .WithWatchdogId(_watchdog.Id)
            .WithWatchdogWebPageId(_watchdogWebPageId)
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
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                SelectText = true,
                Name = "url.com/page"
            })
            .Build();
        _watchdogWebPageId = _watchdog.WebPages.Single().Id;
        _watchdog.SetScrapingResults(_watchdogWebPageId, ["Another World", "Doom 1"]);
        _watchdog.EnableWebPage(_watchdogWebPageId);

        UnitOfWork.Flush();
    }    
}