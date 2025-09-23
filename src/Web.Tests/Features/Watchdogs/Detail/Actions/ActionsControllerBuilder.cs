using FakeItEasy;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Watchdogs.Detail.Actions;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.Actions;

public class ActionsControllerBuilder
{
    private ICoreBus? _bus;
    
    public ActionsControllerBuilder WithBus(ICoreBus bus)
    {
        _bus = bus;
        return this;
    }    
    
    public ActionsController Build()
    {
        _bus ??= A.Fake<ICoreBus>();

        return new ActionsController(_bus);
    }
}