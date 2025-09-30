using FakeItEasy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;
using MrWatchdog.Web.Infrastructure.Authorizations;
using System.Security.Claims;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.WebPage;

[TestFixture]
public class when_updating_watchdog_web_page_as_unauthorized_user : BaseDatabaseTest
{
    private IActionResult _actionResult = null!;
    private WebPageOverviewModel _model = null!;
    private ICoreBus _bus = null!;    
    private Watchdog _watchdog = null!;
    private long _watchdogWebPageId;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        _bus = A.Fake<ICoreBus>();

        var authorizationService = A.Fake<IAuthorizationService>();
        A.CallTo(() => authorizationService.AuthorizeAsync(
                A<ClaimsPrincipal>._,
                _watchdog.Id,
                A<IAuthorizationRequirement[]>.That.Matches(p => p.OfType<WatchdogOwnerOrSuperAdminRequirement>().Any())
            ))
            .Returns(AuthorizationResult.Failed());

        _model = new WebPageOverviewModelBuilder(UnitOfWork)
            .WithBus(_bus)
            .WithAuthorizationService(authorizationService)
            .WithWatchdogWebPageArgs(new WatchdogWebPageArgs
            {
                WatchdogId = _watchdog.Id,
                WatchdogWebPageId = _watchdogWebPageId,
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
        A.CallTo(() => _bus.Send(new UpdateWatchdogWebPageCommand(_model.WatchdogWebPageArgs)))
            .MustNotHaveHappened();
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
                Name = "url.com/page"
            })
            .Build();
        _watchdogWebPageId = _watchdog.WebPages.Single().Id;
    }    
}