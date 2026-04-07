using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
using CoreDdd.Nhibernate.Register.DependencyInjection;
using CoreDdd.Queries;
using CoreDdd.Register.DependencyInjection;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Rebus.Bus;
using Rebus.Bus.Advanced;
using Rebus.Handlers;
using Rebus.Transport.InMem;

namespace CoreBackend.Register.ServiceProvider;

/// <summary>
/// Extension methods on <see cref="IServiceCollection"/> for registering CoreBackend services.
/// </summary>
public static class CoreBackendServicesServiceProviderRegistrationExtensions
{
    extension(IServiceCollection hostedServiceServices)
    {
        /// <summary>
        /// Bridges generic infrastructure and CoreBackend services from a main ASP.NET Core
        /// <see cref="IServiceProvider"/> into a new <see cref="IServiceCollection"/>.
        /// </summary>
        public void AddCoreBackendServicesFromMainServiceProvider(IServiceProvider mainServiceProvider)
        {
            hostedServiceServices.AddSingleton(mainServiceProvider.GetRequiredService<INhibernateConfigurator>());
            hostedServiceServices.AddSingleton(mainServiceProvider.GetRequiredService<IConfiguration>());
            hostedServiceServices.AddSingleton(mainServiceProvider.GetRequiredService<ILoggerFactory>());
            hostedServiceServices.AddSingleton(new MainServiceProviderHolder(mainServiceProvider));
            hostedServiceServices.AddSingleton(typeof(IOptions<>), typeof(MainServiceProviderOptions<>));
            hostedServiceServices.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            hostedServiceServices.AddSingleton(mainServiceProvider.GetRequiredService<IHttpClientFactory>());
            hostedServiceServices.AddSingleton(mainServiceProvider.GetRequiredService<IHostEnvironment>());
            hostedServiceServices.AddSingleton(mainServiceProvider.GetRequiredService<HybridCache>());
            hostedServiceServices.AddSingleton(mainServiceProvider.GetRequiredService<IMessageTypeGetter>());
            hostedServiceServices.AddSingleton(mainServiceProvider.GetRequiredService<ISharedTranslationsGetter>());

            var rebusInMemoryNetwork = mainServiceProvider.GetService<InMemNetwork>();
            if (rebusInMemoryNetwork != null)
            {
                hostedServiceServices.AddSingleton(rebusInMemoryNetwork);
            }
        }

        /// <summary>
        /// Registers all CoreBackend services into Castle Windsor.
        /// </summary>
        public void AddCoreDddAndCoreBackendServices<TNhibernateConfigurator>(
            IConfiguration configuration,
            IBus fireAndForgetWebBus,
            TNhibernateConfigurator nhibernateConfigurator
        ) where TNhibernateConfigurator : class, INhibernateConfigurator
        {
            hostedServiceServices.AddCoreDdd();
            hostedServiceServices.AddCoreDddNhibernate<TNhibernateConfigurator>(_ => nhibernateConfigurator);

            hostedServiceServices.AddCoreBackendRepositoriesAndQueryHandlers();

            hostedServiceServices.AddSingleton<IJobRepositoryFactory, JobRepositoryFactory>();

            // RebusBusHolder — populated after bus.Start()
            hostedServiceServices.AddSingleton<RebusBusHolder>();
            hostedServiceServices.AddSingleton<IBus>(sp => sp.GetRequiredService<RebusBusHolder>().Bus
                                              ?? throw new InvalidOperationException("RebusBusHolder.Bus has not been set. Bus not started yet."));
            hostedServiceServices.AddSingleton<ISyncBus>(sp => sp.GetRequiredService<IBus>().Advanced.SyncBus);

            // Email sender chain
            var emailSenderChainService = configuration["EmailSenderChain:Service"];
            switch (emailSenderChainService)
            {
                case nameof(NullEmailSenderChain):
                    hostedServiceServices.AddSingleton<IEmailSenderChain, NullEmailSenderChain>();
                    break;
                case nameof(EmailSenderChain):
                case null:
                    hostedServiceServices.AddSingleton<IEmailSenderChain, EmailSenderChain>();
                    break;
                default:
                    throw new NotSupportedException($"Email sender chain service {emailSenderChainService} not supported.");
            }

            // Email senders
            hostedServiceServices.Scan(scan => scan
                .FromAssemblyOf<IEmailSender>()
                .AddClasses(classes => classes.AssignableTo<IEmailSender>())
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
            );

            hostedServiceServices.AddScoped<IJobCreator, ExistingTransactionJobCreator>();
            hostedServiceServices.AddKeyedSingleton<IJobCreator, NewTransactionJobCreator>(nameof(NewTransactionJobCreator));
            hostedServiceServices.AddScoped<ICoreBus, CoreBus>();

            // Named CoreBus with NewTransactionJobCreator and fire-and-forget web bus
            hostedServiceServices.AddKeyedSingleton<ICoreBus>(
                RebusConstants.CoreBusWithNewTransactionJobCreatorAndFireAndForgetWebBus,
                (sp, _) => new CoreBus(
                    fireAndForgetWebBus,
                    sp.GetRequiredKeyedService<IJobCreator>(nameof(NewTransactionJobCreator)),
                    sp.GetRequiredService<IActingUserAccessor>(),
                    sp.GetRequiredService<IRequestIdAccessor>(),
                    sp.GetRequiredService<IRebusQueueRedirector>()
                )
            );

            // Accessors
            hostedServiceServices.AddSingleton<IActingUserAccessor, JobContextActingUserAccessor>();
            hostedServiceServices.AddSingleton<IRequestIdAccessor, JobContextRequestIdAccessor>();
            hostedServiceServices.AddSingleton<IRebusQueueRedirector, JobContextRebusQueueRedirector>();
            hostedServiceServices.AddSingleton<IRebusHandlingQueueGetter, RebusHandlingQueueGetter>();

            // Domain event handlers from CoreBackend
            hostedServiceServices.Scan(scan => scan
                .FromAssemblies(typeof(SendDomainEventOverMessageBusDomainEventHandler<>).Assembly)
                .AddClasses(classes => classes.AssignableTo(typeof(IDomainEventHandler<>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime()
            );

            // Rebus message handlers from CoreBackend
            hostedServiceServices.Scan(scan => scan
                .FromAssemblyOf<SendEmailCommandMessageHandler>()
                .AddClasses(classes => classes.AssignableTo(typeof(IHandleMessages<>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime()
            );
        }

        public void AddCoreBackendRepositoriesAndQueryHandlers()
        {
            hostedServiceServices.Scan(scan => scan
                .FromAssemblyOf<JobRepository>()
                .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime()
            );
            hostedServiceServices.Scan(scan => scan
                .FromAssemblyOf<GetJobQueryHandler>()
                .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime()
            );
        }
    }
}