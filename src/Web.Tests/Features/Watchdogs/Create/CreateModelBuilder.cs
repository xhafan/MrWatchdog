using FakeItEasy;
using MrWatchdog.Web.Features.Watchdogs.Create;
using Rebus.Bus;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Create;

public class CreateModelBuilder
{
    private IBus? _bus;
    private string? _name;

    public CreateModelBuilder WithBus(IBus bus)
    {
        _bus = bus;
        return this;
    }
    
    public CreateModelBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public CreateModel Build()
    {
        _bus ??= A.Fake<IBus>();

        var model = new CreateModel(_bus)
        {
            Name = _name!
        };
        ModelValidator.ValidateModel(model);
        return model;
    }
}