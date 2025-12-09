using FakeItEasy;
using MrWatchdog.Core.Infrastructure.ActingUserAccessors;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Scrapers.Create;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Create;

public class CreateModelBuilder
{
    private ICoreBus? _bus;
    private IActingUserAccessor? _actingUserAccessor;
    private string? _name;

    public CreateModelBuilder WithBus(ICoreBus bus)
    {
        _bus = bus;
        return this;
    }
    
    public CreateModelBuilder WithActingUserAccessor(IActingUserAccessor actingUserAccessor)
    {
        _actingUserAccessor = actingUserAccessor;
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
        _actingUserAccessor ??= A.Fake<IActingUserAccessor>();

        var model = new CreateModel(_bus, _actingUserAccessor)
        {
            Name = _name!
        };
        ModelValidator.ValidateModel(model);
        return model;
    }
}