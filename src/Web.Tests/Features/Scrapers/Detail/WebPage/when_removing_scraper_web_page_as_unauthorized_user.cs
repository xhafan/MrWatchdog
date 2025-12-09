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
public class when_removing_watchdog_web_page_as_unauthorized_user : BaseDatabaseTest
{
    private WebPageModel _model = null!;
    private Watchdog _watchdog = null!;
    private ICoreBus _bus = null!;
    private IActionResult _actionResult = null!;
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

        _model = new WebPageModelBuilder(UnitOfWork)
            .WithBus(_bus)
            .WithAuthorizationService(authorizationService)
            .Build();
        
        _actionResult = await _model.OnPostRemoveWatchdogWebPage(_watchdog.Id, _watchdogWebPageId);
    }

    [Test]
    public void command_is_not_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(new RemoveWatchdogWebPageCommand(_watchdog.Id, _watchdogWebPageId)))
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