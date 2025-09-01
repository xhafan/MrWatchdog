using System.Data;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using CoreDdd.Domain.Events;
using CoreDdd.Nhibernate.Configurations;
using CoreDdd.Nhibernate.Register.Castle;
using CoreDdd.Rebus.UnitOfWork;
using CoreDdd.Register.Castle;
using CoreDdd.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Infrastructure;
using MrWatchdog.Core.Infrastructure.ActingUserAccessors;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Messages;
using Rebus.CastleWindsor;
using Rebus.Config;
using Rebus.Pipeline;
using Rebus.Pipeline.Receive;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;
using Rebus.Transport.InMem;

namespace MrWatchdog.Web.HostedServices;

public class RebusHostedService(
    string environmentInputQueueName,
    INhibernateConfigurator nhibernateConfigurator,
    IWindsorContainer mainWindsorContainer,
    IConfiguration configuration
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

        _hostedServiceWindsorContainer.Register(
            Component.For<IJobCreator>().ImplementedBy<ExistingTransactionJobCreator>().LifeStyle.PerRebusMessage(),
            Component.For<IJobCreator>().ImplementedBy<NewTransactionJobCreator>().LifeStyle.Singleton.Named(nameof(NewTransactionJobCreator)),
            Component.For<ICoreBus>().ImplementedBy<CoreBus>().LifeStyle.PerRebusMessage(),
            Component.For<IActingUserAccessor>().ImplementedBy<JobContextActingUserAccessor>().LifeStyle.PerRebusMessage(),
            Classes
                .FromAssemblyContaining(typeof(SendDomainEventOverMessageBusDomainEventHandler<>))
                .BasedOn(typeof(IDomainEventHandler<>))
                .WithService.FirstInterface()
                .Configure(x => x.LifestyleTransient())
        );
        
        _hostedServiceWindsorContainer.AutoRegisterHandlersFromAssemblyOf<CreateWatchdogCommandMessageHandler>();
        
        var rebusUnitOfWork = new RebusUnitOfWork(
            unitOfWorkFactory: _hostedServiceWindsorContainer.Resolve<IUnitOfWorkFactory>(),
            isolationLevel: IsolationLevel.ReadCommitted
        );

        var rebusConfigurer = Configure.With(new CastleWindsorContainerAdapter(_hostedServiceWindsorContainer))
            .Logging(x => x.MicrosoftExtensionsLogging(_hostedServiceWindsorContainer.Resolve<ILoggerFactory>()));
        
        switch (configuration["Rebus:Transport"])
        {
            case "RabbitMq":
                rebusConfigurer.Transport(x => x.UseRabbitMq("amqp://guest:guest@localhost", environmentInputQueueName));
                break;
                
            case "PostgreSql":
                // todo: command handler which sends a domain event over the message bus sends the domain event even when the command handler throws: https://github.com/rebus-org/Rebus.PostgreSql/issues/53
                rebusConfigurer
                    .Transport(x => x.UsePostgreSql(
                        configuration.GetConnectionString("Database"),
                        "RebusQueue",
                        environmentInputQueueName
                    ));
                break;
                
            case "InMemory":
                var rebusInMemoryNetwork = mainWindsorContainer.Resolve<InMemNetwork>();
                rebusConfigurer.Transport(x => x.UseInMemoryTransport(rebusInMemoryNetwork, environmentInputQueueName, registerSubscriptionStorage: false));
                break;
                
            default:
                throw new InvalidOperationException("Rebus transport is not configured. Set Rebus:Transport to RabbitMq, PostgreSql, or InMemory in appsettings.json.");
        }        
        
        rebusConfigurer
            .Routing(x =>
            {
                x.TypeBased()
                    .MapAssemblyDerivedFrom<Command>(environmentInputQueueName)
                    .MapAssemblyDerivedFrom<DomainEvent>(environmentInputQueueName);
            })
            .Options(x =>
                {
                    x.EnableAsyncUnitOfWork(
                        rebusUnitOfWork.CreateAsync,
                        rebusUnitOfWork.CommitAsync,
                        rebusUnitOfWork.RollbackAsync,
                        rebusUnitOfWork.CleanupAsync
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
                            nhibernateConfigurator,
                            _hostedServiceWindsorContainer.Resolve<ILogger<JobTrackingIncomingStep>>(),
                            _hostedServiceWindsorContainer,
                            _hostedServiceWindsorContainer.Resolve<IJobCreator>(nameof(NewTransactionJobCreator))
                        );

                        var jobCompletionIncomingStep = new JobCompletionIncomingStep(
                            _hostedServiceWindsorContainer.Resolve<IJobRepositoryFactory>()
                        );

                        var pipeline = resolutionContext.Get<IPipeline>();
                        return new PipelineStepInjector(pipeline)
                                .OnReceive(jobTrackingIncomingStep, PipelineRelativePosition.After, typeof(DeserializeIncomingMessageStep))
                                .OnReceive(jobCompletionIncomingStep, PipelineRelativePosition.Before, typeof(ActivateHandlersStep))
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