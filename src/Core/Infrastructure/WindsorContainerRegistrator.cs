using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using CoreDdd.Queries;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using MrWatchdog.Core.Features.Jobs.Queries;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.Core.Infrastructure;

public static class WindsorContainerRegistrator
{
    public static void RegisterCommonServices(
        IWindsorContainer windsorContainer, 
        IConfiguration configuration
    )
    {
        windsorContainer.Register(
            Classes
                .FromAssemblyContaining<JobRepository>()
                .BasedOn(typeof(IRepository<>))
                .WithService.AllInterfaces()
                .Configure(x => x.LifestyleTransient()),
            Classes
                .FromAssemblyContaining<GetJobQueryHandler>()
                .BasedOn(typeof(IQueryHandler<>))
                .WithService.AllInterfaces()
                .Configure(x => x.LifestyleTransient()),
            Component.For<IJobRepositoryFactory>().AsFactory()
        );

        var emailSenderService = configuration["EmailSender:Service"];
        switch (emailSenderService)
        {
            case nameof(SmtpServerEmailSender):
                windsorContainer.Register(Component.For<IEmailSender>()
                    .ImplementedBy<SmtpServerEmailSender>().LifeStyle.Singleton
                );
                break;
            case nameof(NullEmailSender):
                windsorContainer.Register(Component.For<IEmailSender>()
                    .ImplementedBy<NullEmailSender>().LifeStyle.Singleton
                );
                break;
            case nameof(SmtpClientDirectlyToRecipientMailServerEmailSender):
            case null:
                windsorContainer.Register(Component.For<IEmailSender>()
                    .ImplementedBy<SmtpClientDirectlyToRecipientMailServerEmailSender>().LifeStyle.Singleton
                );
                break;
            default:
                throw new NotSupportedException($"Email sender service {emailSenderService} not supported.");
        }
    }

    public static void RegisterServicesFromMainWindsorContainer(IWindsorContainer windsorContainer, IWindsorContainer mainWindsorContainer)
    {
        windsorContainer.Register(
            Component.For<ILoggerFactory>()
                .Instance(mainWindsorContainer.Resolve<ILoggerFactory>()),
            Component.For(typeof(ILogger<>))
                .UsingFactoryMethod((_, creationContext) => mainWindsorContainer.Resolve(typeof(ILogger<>).MakeGenericType(creationContext.GenericArguments[0]))),
            
            Component.For<IHttpClientFactory>()
                .Instance(mainWindsorContainer.Resolve<IHttpClientFactory>()),
            
            Component.For(typeof(IOptions<>))
                .UsingFactoryMethod((_, creationContext) => mainWindsorContainer.Resolve(typeof(IOptions<>).MakeGenericType(creationContext.GenericArguments[0]))),

            Component.For<IHostEnvironment>()
                .Instance(mainWindsorContainer.Resolve<IHostEnvironment>()),

            Component
                .For<IPlaywright>()
                .Instance(mainWindsorContainer.Resolve<IPlaywright>())
        );
    }
}