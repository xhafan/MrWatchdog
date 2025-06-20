﻿using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using CoreDdd.Nhibernate.Configurations;
using CoreDdd.Nhibernate.Register.Castle;
using CoreDdd.Rebus.UnitOfWork;
using CoreDdd.Register.Castle;
using CoreDdd.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Infrastructure;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Messages;
using Rebus.CastleWindsor;
using Rebus.Config;
using Rebus.Pipeline;
using Rebus.Pipeline.Receive;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;
using Rebus.Transport.InMem;

namespace MrWatchdog.Web;

public class RebusHostedService(
    string environmentInputQueueName,
    InMemNetwork rebusInMemoryNetwork,
    INhibernateConfigurator nhibernateConfigurator,
    IWindsorContainer mainWindsorContainer
) : IHostedService
{
    private WindsorContainer? _hostedServiceWindsorContainer;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async Task StartAsync(CancellationToken cancellationToken)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        _hostedServiceWindsorContainer = new WindsorContainer(); // Rebus hosted service instance needs its own Windsor container instance

        _hostedServiceWindsorContainer.Register(
            Component.For<INhibernateConfigurator>()
                .Instance(nhibernateConfigurator)
        );
        
        CoreDddNhibernateInstaller.SetUnitOfWorkLifeStyle(x => x.PerRebusMessage());

        _hostedServiceWindsorContainer.Install(
            FromAssembly.Containing<CoreDddInstaller>(),
            FromAssembly.Containing<CoreDddNhibernateInstaller>()
        );        
        
        WindsorContainerRegistrator.RegisterCommonServices(_hostedServiceWindsorContainer);
        WindsorContainerRegistrator.RegisterServicesFromMainWindsorContainer(_hostedServiceWindsorContainer, mainWindsorContainer);
        
        _hostedServiceWindsorContainer.AutoRegisterHandlersFromAssemblyOf<CreateWatchdogCommandMessageHandler>();
        
        var rebusUnitOfWork = new RebusUnitOfWork(
            unitOfWorkFactory: _hostedServiceWindsorContainer.Resolve<IUnitOfWorkFactory>(),
            isolationLevel: System.Data.IsolationLevel.ReadCommitted
        );

        Configure.With(new CastleWindsorContainerAdapter(_hostedServiceWindsorContainer))
            .Logging(x => x.MicrosoftExtensionsLogging(_hostedServiceWindsorContainer.Resolve<ILoggerFactory>()))
            .Transport(x => x.UseInMemoryTransport(rebusInMemoryNetwork, environmentInputQueueName, registerSubscriptionStorage: false))
            .Routing(x =>
            {
                x.TypeBased()
                    .MapAssemblyDerivedFrom<Command>(environmentInputQueueName)
                    .MapAssemblyDerivedFrom<DomainEvent>(environmentInputQueueName);
            })
            .Options(x =>
                {
                    x.EnableUnitOfWork(
                        rebusUnitOfWork.Create,
                        rebusUnitOfWork.Commit,
                        rebusUnitOfWork.Rollback,
                        rebusUnitOfWork.Cleanup
                    );
                    x.RetryStrategy(
                        errorQueueName: $"{environmentInputQueueName}Error",
                        maxDeliveryAttempts: RebusConstants.MaxDeliveryAttempts
                    );
                    x.SetNumberOfWorkers(5);
                    x.SetMaxParallelism(5);
                    x.Decorate<IPipeline>(resolutionContext =>
                    {
                        var jobTrackingIncomingStep = new JobTrackingIncomingStep(
                            _hostedServiceWindsorContainer.Resolve<INhibernateConfigurator>(),
                            _hostedServiceWindsorContainer.Resolve<ILogger<JobTrackingIncomingStep>>(),
                            _hostedServiceWindsorContainer
                        );

                        var pipeline = resolutionContext.Get<IPipeline>();
                        return new PipelineStepInjector(pipeline)
                                .OnReceive(jobTrackingIncomingStep, PipelineRelativePosition.After, typeof(DeserializeIncomingMessageStep))
                            ;
                    });
                }
            )
            .Start();
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async Task StopAsync(CancellationToken cancellationToken)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        _hostedServiceWindsorContainer?.Dispose();
    }
}