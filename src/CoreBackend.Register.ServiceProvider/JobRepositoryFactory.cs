using CoreBackend.Infrastructure.Rebus;
using CoreBackend.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Pipeline;

namespace CoreBackend.Register.ServiceProvider;

/// <summary>
/// ServiceProvider implementation of IJobRepositoryFactory.
/// Resolves IJobRepository from the per-message DI scope saved in IncomingStepContext.
/// </summary>
public class JobRepositoryFactory : IJobRepositoryFactory
{
    public IJobRepository Create()
    {
        var scope = MessageContext.Current?.IncomingStepContext.Load<AsyncServiceScope?>()
            ?? throw new InvalidOperationException("No per-message DI scope found in IncomingStepContext.");
        return scope.ServiceProvider.GetRequiredService<IJobRepository>();
    }
}
