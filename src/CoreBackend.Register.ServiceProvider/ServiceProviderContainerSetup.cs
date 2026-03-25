using System.Data;
using CoreBackend.Infrastructure.Rebus;
using CoreDdd.UnitOfWorks;
using CoreIoC;
using CoreIoC.ServiceProvider;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Bus;
using Rebus.Config;
using Rebus.ServiceProvider;

namespace CoreBackend.Register.ServiceProvider;

/// <summary>
/// Creates AsyncServiceScope per message and saves it in IncomingStepContext so
/// DependencyInjectionHandlerActivator reuses it (matching ASP.NET Core's per-request scope pattern).
/// </summary>
public class ServiceProviderContainerSetup(IServiceProvider hostedServiceServiceProvider) : IRebusHostedServiceContainerSetup
{
    private readonly ServiceProviderContainer _container = new(hostedServiceServiceProvider);

    public Func<RebusConfigurer> CreateRebusConfigurerFactory()
    {
        return () => Configure.With(new DependencyInjectionHandlerActivator(hostedServiceServiceProvider));
    }

    public IContainer GetContainer()
    {
        return _container;
    }

    public void ConfigureUnitOfWork(OptionsConfigurer options)
    {
        options.EnableAsyncUnitOfWork<IUnitOfWork>(
            create: messageContext =>
            {
                var scope = hostedServiceServiceProvider.CreateAsyncScope();
                messageContext.IncomingStepContext.Save<AsyncServiceScope?>(scope);

                JobContext.IoCContainer.Value.Value = new ServiceProviderContainer(scope.ServiceProvider);
                
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                unitOfWork.BeginTransaction(IsolationLevel.ReadCommitted);
                return Task.FromResult(unitOfWork);
            },
            commit: async (_, unitOfWork) =>
            {
                await unitOfWork.CommitAsync();
            },
            rollback: async (_, unitOfWork) =>
            {
                await unitOfWork.RollbackAsync();
            },
            dispose: async (messageContext, _) =>
            {
                var scope = messageContext.IncomingStepContext.Load<AsyncServiceScope?>();
                if (scope.HasValue)
                {
                    await scope.Value.DisposeAsync();
                }

                JobContext.IoCContainer.Value.Value = null;
            }
        );
    }

    public void OnBusStarted(IBus bus)
    {
        hostedServiceServiceProvider.GetRequiredService<RebusBusHolder>().Bus = bus;
    }

    public async Task OnServiceStopped()
    {
        if (hostedServiceServiceProvider is not IAsyncDisposable serviceServiceProvider) return;
        
        await serviceServiceProvider.DisposeAsync();
    }
}
