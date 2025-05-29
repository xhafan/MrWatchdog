using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Web.Features.Watchdogs.Create;
using Rebus.Bus;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Create;

[TestFixture]
public class when_creating_new_watchdog_with_name
{
    private IActionResult _actionResult = null!;
    private CreateModel _model = null!;
    private IBus _bus = null!;

    [SetUp]
    public async Task Context()
    {
        _bus = A.Fake<IBus>();
        
        _model = new CreateModelBuilder()
            .WithName("watchdog name")
            .WithBus(_bus)
            .Build();
        
        _actionResult = await _model.OnPost();
    }

    [Test]
    public void command_is_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(
                A<CreateWatchdogCommand>.That.Matches(p => p.Name == "watchdog name" 
                                                           && !p.Guid.Equals(Guid.Empty)),
                A<IDictionary<string, string>>._
            )
        ).MustHaveHappenedOnceExactly();
    }
    
    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<OkObjectResult>();
        var okObjectResult = (OkObjectResult) _actionResult;
        var value = okObjectResult.Value;
        value.ShouldBeOfType<string>();
        var valueAsString = (string) value;
        valueAsString.ShouldMatch(@"[0-9A-Fa-f\-]{36}");
    }

    [Test]
    public void model_is_valid()
    {
        _model.ModelState.IsValid.ShouldBe(true);
    }
}