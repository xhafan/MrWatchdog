using FakeItEasy;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Watchdogs.Create;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Create;

public class CreateModelBuilder
{
    private ICoreBus? _bus;
    private string? _name;

    public CreateModelBuilder WithBus(ICoreBus bus)
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
        _bus ??= A.Fake<ICoreBus>();

        var model = new CreateModel(_bus)
        {
            Name = _name!
        };
        ModelValidator.ValidateModel(model);
        return model;
    }
}