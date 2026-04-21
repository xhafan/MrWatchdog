using CoreBackend.Infrastructure.Rebus;
using CoreBackend.TestsShared;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Web.Features.Watchdogs.Api;
using System.Security.Claims;
using CoreBackend.Account.Features.Account;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Api;

[TestFixture]
public class when_disabling_watchdog_notification_via_post_for_non_existing_watchdog : BaseDatabaseTest
{
    private WatchdogsController _controller = null!;
    private ICoreBus _bus = null!;
    private IActionResult _actionResult = null!;
    
    [SetUp]
    public async Task Context()
    {
        _bus = A.Fake<ICoreBus>();
        
        _controller = new WatchdogsControllerBuilder(UnitOfWork)
            .WithBus(_bus)
            .Build();

        var unsubscribeToken = TokenGenerator.GenerateUnsubscribeToken(
            [new Claim(CustomClaimTypes.WatchdogId, "123")], 
            OptionsTestRetriever.Retrieve<JwtOptions>().Value
        );
        
        _actionResult = await _controller.DisableNotificationPost(unsubscribeToken);
    }

    [Test]
    public void command_is_not_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(new DisableWatchdogNotificationCommand(123))).MustNotHaveHappened();
    }
    
    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<NotFoundResult>();
    }
}