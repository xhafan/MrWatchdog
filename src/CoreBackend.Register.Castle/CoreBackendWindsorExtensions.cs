using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using CoreBackend.Features.Jobs.Queries;
using CoreBackend.Infrastructure.ActingUserAccessors;
using CoreBackend.Infrastructure.EmailSenders;
using CoreBackend.Infrastructure.Rebus;
using CoreBackend.Infrastructure.Rebus.RebusQueueRedirectors;
using CoreBackend.Infrastructure.Repositories;
using CoreBackend.Infrastructure.RequestIdAccessors;
using CoreBackend.Messages;
using CoreBackend.Resources;
using CoreDdd.Domain.Events;
using CoreDdd.Domain.Repositories;
using CoreDdd.Nhibernate.Configurations;
using CoreDdd.Nhibernate.Register.Castle;
using CoreDdd.Queries;
using CoreDdd.Register.Castle;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using Rebus.CastleWindsor;
using Rebus.Transport.InMem;

namespace CoreBackend.Register.Castle;

/// <summary>
/// Extension methods on <see cref="IWindsorContainer"/> for registering CoreBackend services.
/// </summary>
public static class CoreBackendWindsorExtensions
{
    extension(IWindsorContainer hostedServiceWindsorContainer)
    {
        /// <summary>
        /// Bridges generic infrastructure and CoreBackend services from an ASP.NET Core
        /// <see cref="IServiceProvider"/> into a Castle Windsor container.
        /// </summary>
        public void AddCoreBackendServicesFromMainServiceProvider(IServiceProvider mainServiceProvider)
        {
            hostedServiceWindsorContainer.Register(
                Component.For<INhibernateConfigurator>()
                    .Instance(mainServiceProvider.GetRequiredService<INhibernateConfigurator>()),

                Component.For<IConfiguration>()
                    .Instance(mainServiceProvider.GetRequiredService<IConfiguration>()),

                Component.For<ILoggerFactory>()
                    .Instance(mainServiceProvider.GetRequiredService<ILoggerFactory>()),

                Component.For(typeof(ILogger<>))
                    .UsingFactoryMethod((_, creationContext) =>
                        mainServiceProvider.GetRequiredService(
                            typeof(ILogger<>).MakeGenericType(creationContext.GenericArguments[0]))),

                Component.For<IHttpClientFactory>()
                    .Instance(mainServiceProvider.GetRequiredService<IHttpClientFactory>()),

                Component.For(typeof(IOptions<>))
                    .UsingFactoryMethod((_, creationContext) =>
                        mainServiceProvider.GetRequiredService(
                            typeof(IOptions<>).MakeGenericType(creationContext.GenericArguments[0]))),

                Component.For<IHostEnvironment>()
                    .Instance(mainServiceProvider.GetRequiredService<IHostEnvironment>()),

                Component.For<HybridCache>()
                    .Instance(mainServiceProvider.GetRequiredService<HybridCache>()),

                Component.For<IMessageTypeGetter>()
                    .Instance(mainServiceProvider.GetRequiredService<IMessageTypeGetter>()),

                Component.For<ISharedTranslationsGetter>()
                    .Instance(mainServiceProvider.GetRequiredService<ISharedTranslationsGetter>())
            );

            var rebusInMemoryNetwork = mainServiceProvider.GetService<InMemNetwork>();
            if (rebusInMemoryNetwork != null)
            {
                hostedServiceWindsorContainer.Register(Component.For<InMemNetwork>().Instance(rebusInMemoryNetwork));
            }
        }

        /// <summary>
        /// Registers CoreBackend services into Castle Windsor.
        /// </summary>
        public void AddCoreDddAndCoreBackendServices(IBus fireAndForgetWebBus)
        {
            CoreDddNhibernateInstaller.SetUnitOfWorkLifeStyle(x => x.PerRebusMessage());

            hostedServiceWindsorContainer.Install(
                FromAssembly.Containing<CoreDddInstaller>(),
                FromAssembly.Containing<CoreDddNhibernateInstaller>()
            );

            hostedServiceWindsorContainer.Register(
                Classes
                    .FromAssemblyContaining<JobRepository>()
                    .BasedOn(typeof(IRepository<>))
                    .WithService.AllInterfaces()
                    .Configure(x => x.LifestyleTransient()),
                Classes
                    .FromAssemblyContaining<GetJobQueryHandler>()
                    .BasedOn(typeof(IQueryHandler<,>))
                    .WithService.AllInterfaces()
                    .Configure(x => x.LifestyleTransient())
            );

            hostedServiceWindsorContainer.Register(
                Component.For<IJobRepositoryFactory>().AsFactory()
            );

            // Email sender chain
            var configuration = hostedServiceWindsorContainer.Resolve<IConfiguration>();
            var emailSenderChainService = configuration["EmailSenderChain:Service"];
            switch (emailSenderChainService)
            {
                case nameof(NullEmailSenderChain):
                    hostedServiceWindsorContainer.Register(
                        Component.For<IEmailSenderChain>().ImplementedBy<NullEmailSenderChain>().LifeStyle.Singleton
                    );
                    break;
                case nameof(EmailSenderChain):
                case null:
                    hostedServiceWindsorContainer.Register(
                        Component.For<IEmailSenderChain>().ImplementedBy<EmailSenderChain>().LifeStyle.Singleton
                    );
                    break;
                default:
                    throw new NotSupportedException($"Email sender chain service {emailSenderChainService} not supported.");
            }

            // Email senders
            hostedServiceWindsorContainer.Register(
                Classes
                    .FromAssemblyContaining<IEmailSender>()
                    .BasedOn<IEmailSender>()
                    .WithService.FromInterface()
                    .Configure(c => c.DependsOn(Dependency.OnValue<Func<SmtpClient>>(null)))
                    .LifestyleSingleton()
            );

            hostedServiceWindsorContainer.Register(
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

                // Domain event handlers from CoreBackend
                Classes
                    .FromAssemblyContaining(typeof(SendDomainEventOverMessageBusDomainEventHandler<>))
                    .BasedOn(typeof(IDomainEventHandler<>))
                    .WithService.FirstInterface()
                    .Configure(x => x.LifestyleTransient())
            );

            hostedServiceWindsorContainer.Kernel.Resolver.AddSubResolver(new CollectionResolver(hostedServiceWindsorContainer.Kernel));

            // Rebus message handlers from CoreBackend
            hostedServiceWindsorContainer.AutoRegisterHandlersFromAssemblyOf<SendEmailCommandMessageHandler>();
        }
    }
}
