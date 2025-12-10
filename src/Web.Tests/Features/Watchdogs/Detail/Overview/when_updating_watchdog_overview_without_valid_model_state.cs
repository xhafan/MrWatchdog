using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Watchdogs.Detail.Overview;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.Overview;

[TestFixture]
public class when_updating_watchdog_overview_without_valid_model_state : BaseDatabaseTest
{
    private IActionResult _actionResult = null!;
    private OverviewModel _model = null!;
    private ICoreBus _bus = null!;

    [SetUp]
    public async Task Context()
    {
        _bus = A.Fake<ICoreBus>();
        
        _model = new OverviewModelBuilder(UnitOfWork)
            .WithBus(_bus)
            .WithWatchdogOverviewArgs(
                new WatchdogOverviewArgs
                {
                    WatchdogId = 0,
                    ReceiveNotification = true,
                    SearchTerm = null
                }
            )
            .Build();

        _actionResult = await _model.OnPost();
    }

    [Test]
    public void command_is_not_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(A<UpdateWatchdogOverviewCommand>._)).MustNotHaveHappened();
    }
    
    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<PageResult>();
        var pageResult = (PageResult) _actionResult;
        pageResult.StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
    }
    
    [Test]
    public void model_is_invalid()
    {
        _model.ModelState.IsValid.ShouldBe(false);
        var watchdogOverviewArgsWatchdogIdErrors = _model
            .ModelState[$"{nameof(OverviewModel.WatchdogOverviewArgs)}.{nameof(OverviewModel.WatchdogOverviewArgs.WatchdogId)}"]?.Errors;
        watchdogOverviewArgsWatchdogIdErrors.ShouldNotBeNull();
        watchdogOverviewArgsWatchdogIdErrors.ShouldContain(x => x.ErrorMessage.Contains("default value"));
    }    
}