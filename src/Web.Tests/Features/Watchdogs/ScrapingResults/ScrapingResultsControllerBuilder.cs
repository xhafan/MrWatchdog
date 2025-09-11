using FakeItEasy;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Watchdogs.ScrapingResults;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.ScrapingResults;

public class ScrapingResultsControllerBuilder
{
    private ICoreBus? _bus;
    
    public ScrapingResultsControllerBuilder WithBus(ICoreBus bus)
    {
        _bus = bus;
        return this;
    }    
    
    public ScrapingResultsController Build()
    {
        _bus ??= A.Fake<ICoreBus>();

        return new ScrapingResultsController(_bus);
    }
}