using CoreBackend.Infrastructure.Rebus;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Api;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Api;

[TestFixture]
public class when_disabling_watchdog_notification_via_get_for_existing_watchdog : BaseDatabaseTest
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
        
        var unsubscribeToken = TokenGenerator.GenerateUnsubscribeToken(_watchdog.Id, OptionsTestRetriever.Retrieve<JwtOptions>().Value);

        _actionResult = await _controller.DisableNotificationGet(unsubscribeToken);
    }

    [Test]
    public void command_is_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(new DisableWatchdogNotificationCommand(_watchdog.Id))).MustHaveHappenedOnceExactly();
    }
    
    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<RedirectToPageResult>();
        var redirectToPageResult = (RedirectToPageResult) _actionResult;
        redirectToPageResult.PageName.ShouldBe("/Watchdogs/Unsubscribed/Unsubscribed");
    }

    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork).Build();
    }
}