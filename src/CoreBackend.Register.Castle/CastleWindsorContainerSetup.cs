using Castle.Windsor;
using CoreBackend.Infrastructure.Rebus;
using CoreDdd.Rebus.UnitOfWork;
using CoreDdd.UnitOfWorks;
using CoreIoC;
using CoreIoC.Castle;
using Rebus.Bus;
using Rebus.Config;
using System.Data;

namespace CoreBackend.Register.Castle;

public class CastleWindsorContainerSetup(WindsorContainer windsorContainer) : IRebusHostedServiceContainerSetup
{
    private readonly CastleContainer _castleContainer = new(windsorContainer);

    public Func<RebusConfigurer> CreateRebusConfigurerFactory()
    {
        return () => Configure.With(new CastleWindsorContainerAdapter(windsorContainer));
    }

    public IContainer GetContainer()
    {
        return _castleContainer;
    }

    public void ConfigureUnitOfWork(OptionsConfigurer options)
    {
        var rebusUnitOfWork = new RebusUnitOfWork(
            unitOfWorkFactory: _castleContainer.Resolve<IUnitOfWorkFactory>(),
            isolationLevel: IsolationLevel.ReadCommitted
        );

        options.EnableAsyncUnitOfWork(
            create: async messageContext =>
            {
                JobContext.IoCContainer.Value.Value = _castleContainer;

                var unitOfWork = await rebusUnitOfWork.CreateAsync(messageContext);
                return unitOfWork;
            },
            commit: rebusUnitOfWork.CommitAsync,
            rollback: rebusUnitOfWork.RollbackAsync,
            dispose: async (messageContext, unitOfWork) =>
            {
                await rebusUnitOfWork.CleanupAsync(messageContext, unitOfWork);
                
                JobContext.IoCContainer.Value.Value = null;
            }
        );
    }

    public void OnBusStarted(IBus bus)
    {
    }

    public Task OnServiceStopped()
    {
        windsorContainer.Dispose();
        return Task.CompletedTask;
    }
}
