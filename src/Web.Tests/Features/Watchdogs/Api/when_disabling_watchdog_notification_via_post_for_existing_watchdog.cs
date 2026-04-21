using CoreBackend.Infrastructure.Rebus;
using CoreBackend.TestsShared;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Api;
using System.Security.Claims;
using CoreBackend.Account.Features.LoginLink;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Api;

[TestFixture]
public class when_disabling_watchdog_notification_via_post_for_existing_watchdog : BaseDatabaseTest
{
    private WatchdogsController _controller = null!;
    private Watchdog _watchdog = null!;
    private ICoreBus _bus = null!;
    private IActionResult _actionResult = null!;
    
    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _bus = A.Fake<ICoreBus>();
        
        _controller = new WatchdogsControllerBuilder(UnitOfWork)
            .WithBus(_bus)
            .Build();
        
        var unsubscribeToken = TokenGenerator.GenerateUnsubscribeToken(
            [new Claim(CustomClaimTypes.WatchdogId, _watchdog.Id.ToString())],
            OptionsTestRetriever.Retrieve<JwtOptions>().Value
        );

        _actionResult = await _controller.DisableNotificationPost(unsubscribeToken);
    }

    [Test]
    public void command_is_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(new DisableWatchdogNotificationCommand(_watchdog.Id))).MustHaveHappenedOnceExactly();
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
        _watchdog = new WatchdogBuilder(UnitOfWork).Build();
    }
}