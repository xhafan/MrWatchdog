using FakeItEasy;
using MrWatchdog.Web.Pages.Watchdogs;
using Rebus.Bus;

namespace MrWatchdog.Web.Tests.Pages.Watchdogs.Detail;

public class CreateModelBuilder
{
    private IBus? _bus;
    private string? _name;

    public CreateModelBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public CreateModelBuilder WithBus(IBus bus)
    {
        _bus = bus;
        return this;
    }
    
    public CreateModel Build()
    {
        _bus ??= A.Fake<IBus>();

        var createModel = new CreateModel(_bus)
        {
            Name = _name!
        };
        ModelValidator.ValidateModel(createModel);
        return createModel;
    }
}