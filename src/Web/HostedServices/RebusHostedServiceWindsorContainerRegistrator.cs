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
using CoreBackend.Resources;
using CoreDdd.Domain.Events;
using CoreDdd.Domain.Repositories;
using CoreDdd.Nhibernate.Configurations;
using CoreDdd.Nhibernate.Register.Castle;
using CoreDdd.Queries;
using CoreDdd.Register.Castle;
using CoreWeb.Features.Jobs;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using MrWatchdog.Core.Features.Account.Queries;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Services;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Bus;
using Rebus.CastleWindsor;
using Rebus.Transport.InMem;

namespace MrWatchdog.Web.HostedServices;

public static class RebusHostedServiceWindsorContainerRegistrator // todo: try to get rid of Castle Windsor and refactor this into ServiceProvider?
{
    public static void RegisterServices(
        WindsorContainer hostedServiceWindsorContainer,
        IServiceProvider serviceProvider
    )
    {
        _RegisterServicesFromServiceProvider(hostedServiceWindsorContainer, serviceProvider);

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
                .FromAssemblyContaining<UserRepository>()
                .BasedOn(typeof(IRepository<>))
                .WithService.AllInterfaces()
                .Configure(x => x.LifestyleTransient()),
            Classes
                .FromAssemblyContaining<GetJobQueryHandler>()
                .BasedOn(typeof(IQueryHandler<,>))
                .WithService.AllInterfaces()
                .Configure(x => x.LifestyleTransient()),
            Classes
                .FromAssemblyContaining<GetUserByEmailQueryHandler>()
                .BasedOn(typeof(IQueryHandler<,>))
                .WithService.AllInterfaces()
                .Configure(x => x.LifestyleTransient()),
            Component.For<IJobRepositoryFactory>().AsFactory()
        );

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

        hostedServiceWindsorContainer.Register(
            Classes
                .FromAssemblyContaining<IEmailSender>()
                .BasedOn<IEmailSender>()
                .WithService.FromInterface()
                .Configure(c => c.DependsOn(Dependency.OnValue<Func<SmtpClient>>(null)))
                .LifestyleSingleton()
        );

        var fireAndForgetWebBus = serviceProvider.GetRequiredService<IBus>();

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
            Classes
                .FromAssemblyContaining(typeof(SendDomainEventOverMessageBusDomainEventHandler<>))
                .BasedOn(typeof(IDomainEventHandler<>))
                .WithService.FirstInterface()
                .Configure(x => x.LifestyleTransient()),

            Classes
                .FromAssemblyContaining<IWebScraper>()
                .BasedOn<IWebScraper>()
                .WithService.FromInterface()
                .LifestyleTransient(),

            Component.For<IWebScraperChain>()
                .ImplementedBy<WebScraperChain>()
                .LifestyleTransient()
        );

        hostedServiceWindsorContainer.Kernel.Resolver.AddSubResolver(new CollectionResolver(hostedServiceWindsorContainer.Kernel)); // to resolve IEnumerable<T>

        hostedServiceWindsorContainer.AutoRegisterHandlersFromAssemblyOf<SendEmailCommandMessageHandler>();
        hostedServiceWindsorContainer.AutoRegisterHandlersFromAssemblyOf<CreateScraperCommandMessageHandler>();
    }


    private static void _RegisterServicesFromServiceProvider(
        IWindsorContainer hostedServiceWindsorContainer,
        IServiceProvider serviceProvider
    )
    {
        hostedServiceWindsorContainer.Register(

            Component.For<INhibernateConfigurator>()
                .Instance(serviceProvider.GetRequiredService<INhibernateConfigurator>()),
            
            Component.For<IConfiguration>()
                .Instance(serviceProvider.GetRequiredService<IConfiguration>()),
            
            Component.For<ILoggerFactory>()
                .Instance(serviceProvider.GetRequiredService<ILoggerFactory>()),
            
            Component.For(typeof(ILogger<>))
                .UsingFactoryMethod((_, creationContext) => serviceProvider.GetRequiredService(typeof(ILogger<>).MakeGenericType(creationContext.GenericArguments[0]))),
            
            Component.For<IHttpClientFactory>()
                .Instance(serviceProvider.GetRequiredService<IHttpClientFactory>()),
            
            Component.For(typeof(IOptions<>))
                .UsingFactoryMethod((_, creationContext) => serviceProvider.GetRequiredService(typeof(IOptions<>).MakeGenericType(creationContext.GenericArguments[0]))),

            Component.For<IHostEnvironment>()
                .Instance(serviceProvider.GetRequiredService<IHostEnvironment>()),

            Component
                .For<IPlaywright>()
                .Instance(serviceProvider.GetRequiredService<IPlaywright>()),

            Component
                .For<HybridCache>()
                .Instance(serviceProvider.GetRequiredService<HybridCache>()),

            Component
                .For<IMessageTypeGetter>()
                .Instance(serviceProvider.GetRequiredService<IMessageTypeGetter>()),

            Component
                .For<ISharedTranslationsGetter>()
                .Instance(serviceProvider.GetRequiredService<ISharedTranslationsGetter>())

        );

        var rebusInMemoryNetwork = serviceProvider.GetService<InMemNetwork>();
        if (rebusInMemoryNetwork != null)
        {
            hostedServiceWindsorContainer.Register(
                Component.For<InMemNetwork>().Instance(rebusInMemoryNetwork)
            );
        }
    }
}