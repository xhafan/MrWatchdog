using CoreIoC;
using Rebus.Bus;
using Rebus.Config;

namespace CoreBackend.Infrastructure.Rebus;

public interface IRebusHostedServiceContainerSetup
{
    Func<RebusConfigurer> CreateRebusConfigurerFactory();
    IContainer GetContainer();
    void ConfigureUnitOfWork(OptionsConfigurer options);
    void OnBusStarted(IBus bus);
    Task OnServiceStopped();
}
