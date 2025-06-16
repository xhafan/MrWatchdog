using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Watchdogs.Create;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Create;

[TestFixture]
public class when_creating_new_watchdog_with_name
{
    private IActionResult _actionResult = null!;
    private CreateModel _model = null!;
    private ICoreBus _bus = null!;

    [SetUp]
    public async Task Context()
    {
        _bus = A.Fake<ICoreBus>();
        
        _model = new CreateModelBuilder()
            .WithName("watchdog name")
            .WithBus(_bus)
            .Build();
        
        _actionResult = await _model.OnPost();
    }

    [Test]
    public void command_is_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(new CreateWatchdogCommand("watchdog name"))).MustHaveHappenedOnceExactly();
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

    [Test]
    public void model_is_valid()
    {
        _model.ModelState.IsValid.ShouldBe(true);
    }
}