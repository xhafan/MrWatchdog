using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using CoreDdd.Domain.Events;
using CoreDdd.Nhibernate.Configurations;
using CoreDdd.Nhibernate.Register.Castle;
using CoreDdd.Rebus.UnitOfWork;
using CoreDdd.Register.Castle;
using CoreDdd.UnitOfWorks;
using CoreUtils;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Infrastructure;
using MrWatchdog.Core.Infrastructure.ActingUserAccessors;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Rebus.MessageRouting;
using MrWatchdog.Core.Infrastructure.Rebus.RebusQueueRedirectors;
using MrWatchdog.Core.Infrastructure.RequestIdAccessors;
using Rebus.Bus;
using Rebus.CastleWindsor;
using Rebus.Config;
using Rebus.Pipeline;
using Rebus.Pipeline.Receive;
using Rebus.Retry;
using Rebus.Retry.Simple;
using Rebus.Serialization;
using Rebus.Transport.InMem;
using System.Data;

namespace MrWatchdog.Web.HostedServices;

public class RebusHostedService(
    string inputQueueName,
    string environmentName,
    INhibernateConfigurator nhibernateConfigurator,
    IWindsorContainer mainWindsorContainer,
    IConfiguration configuration,
    string connectionString
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

        var fireAndForgetWebBus = mainWindsorContainer.Resolve<IBus>();

        _hostedServiceWindsorContainer.Register(
            Component.For<IJobCreator>().ImplementedBy<ExistingTransactionJobCreator>().LifeStyle.PerRebusMessage(),
            Component.For<IJobCreator>().ImplementedBy<NewTransactionJobCreator>().LifeStyle.Singleton.Named(nameof(NewTransactionJobCreator)),
            Component.For<ICoreBus>().ImplementedBy<CoreBus>().LifeStyle.PerRebusMessage(),
            
            Component.For<ICoreBus>().ImplementedBy<CoreBus>().LifeStyle.Singleton
                .Named(RebusConstants.CoreBusWithNewTransactionJobCreatorAndFireAndForgetWebBus)
                .DependsOn(Dependency.OnComponent(typeof(IJobCreator), nameof(NewTransactionJobCreator)))
                .DependsOn(Dependency.OnValue(typeof(IBus), fireAndForgetWebBus)),
            
            Component.For<IActingUserAccessor>().ImplementedBy<JobContextActingUserAccessor>().LifeStyle.Singleton,
            Component.For<IRequestIdAccessor>().ImplementedBy<JobContextRequestIdAccessor>().LifeStyle.Singleton,
            Component.For<IRebusQueueRedirector>().ImplementedBy<JobContextRebusQueueRedirector>().LifeStyle.Singleton,
            Component.For<IRebusHandlingQueueGetter>().ImplementedBy<RebusHandlingQueueGetter>().LifeStyle.Singleton,
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

        var environmentInputQueueName = $"{environmentName}{inputQueueName}";

        switch (configuration["Rebus:Transport"])
        {
            case "RabbitMq":
                rebusConfigurer.Transport(x => x.UseRabbitMq("amqp://guest:guest@localhost", environmentInputQueueName));
                break;
                
            case "PostgreSql":
                // todo: command handler which sends a domain event over the message bus sends the domain event even when the command handler throws: https://github.com/rebus-org/Rebus.PostgreSql/issues/53
                rebusConfigurer
                    .Transport(x => x.UsePostgreSql(
                        connectionString,
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

        var defaultNumberOfWorkersAsString = configuration["Rebus:DefaultNumberOfWorkers"];
        Guard.Hope(defaultNumberOfWorkersAsString != null, "Rebus:DefaultNumberOfWorkers not set.");
        var numberOfWorkers = int.Parse(defaultNumberOfWorkersAsString);

        var inputQueueNameNumberOfWorkersKey = $"Rebus:{inputQueueName}NumberOfWorkers";
        var inputQueueNameNumberOfWorkersAsString = configuration[inputQueueNameNumberOfWorkersKey];
        if (inputQueueNameNumberOfWorkersAsString != null)
        {
            numberOfWorkers = int.Parse(inputQueueNameNumberOfWorkersAsString);
        }

        rebusConfigurer
            .Routing(configurer => MessageRoutingConfigurator.ConfigureMessageRouting(configurer, environmentName))
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
                        maxDeliveryAttempts: RebusConstants.MaxDeliveryAttempts,
                        errorTrackingMaxAgeMinutes: 1440 // 1440 minutes = 24h; this prevents never-ending number of retries for messages whose handling take a long time
                    );
                    x.SetNumberOfWorkers(numberOfWorkers);
                    x.SetMaxParallelism(numberOfWorkers);
                    x.Decorate<IPipeline>(resolutionContext =>
                    {
                        var messageLoggingIncomingStep = new MessageLoggingIncomingStep(_hostedServiceWindsorContainer.Resolve<ILogger<MessageLoggingIncomingStep>>());
                        
                        var jobTrackingIncomingStep = new JobTrackingIncomingStep(
                            nhibernateConfigurator,
                            _hostedServiceWindsorContainer.Resolve<ILogger<JobTrackingIncomingStep>>(),
                            _hostedServiceWindsorContainer,
                            _hostedServiceWindsorContainer.Resolve<IJobCreator>(nameof(NewTransactionJobCreator)),
                            _hostedServiceWindsorContainer.Resolve<IRebusHandlingQueueGetter>()
                        );

                        var jobCompletionIncomingStep = new JobCompletionIncomingStep(
                            _hostedServiceWindsorContainer.Resolve<IJobRepositoryFactory>()
                        );

                        var pipeline = resolutionContext.Get<IPipeline>();
                        return new PipelineStepInjector(pipeline)
                                .OnReceive(messageLoggingIncomingStep, PipelineRelativePosition.After, typeof(DeserializeIncomingMessageStep))
                                .OnReceive(jobTrackingIncomingStep, PipelineRelativePosition.After, typeof(DeserializeIncomingMessageStep))
                                .OnReceive(jobCompletionIncomingStep, PipelineRelativePosition.Before, typeof(ActivateHandlersStep))
                            ;
                    });

                    x.Decorate<IErrorHandler>(resolutionContext =>
                        new ReportFailedMessageErrorHandler(
                            resolutionContext.Get<IErrorHandler>(),
                            resolutionContext.Get<ISerializer>(),
                            _hostedServiceWindsorContainer.Resolve<ICoreBus>(RebusConstants.CoreBusWithNewTransactionJobCreatorAndFireAndForgetWebBus),
                            _hostedServiceWindsorContainer.Resolve<IOptions<RuntimeOptions>>(),
                            _hostedServiceWindsorContainer.Resolve<IOptions<EmailAddressesOptions>>()
                        )
                    );

                    //x.LogPipeline(verbose: true); // uncomment to log pipeline steps so one can decide where to insert an incoming step
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