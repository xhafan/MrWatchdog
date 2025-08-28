using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using CoreDdd.Queries;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Features.Jobs.Queries;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.Core.Resources;

namespace MrWatchdog.Core.Infrastructure;

public static class WindsorContainerRegistrator
{
    public static void RegisterCommonServices(IWindsorContainer windsorContainer)
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
            Component.For<IJobRepositoryFactory>().AsFactory(),
            Component.For<IEmailSender>().ImplementedBy<EmailSender>().LifeStyle.Singleton
        );
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
            
            Component.For<IStringLocalizer<Resource>>()
                .Instance(mainWindsorContainer.Resolve<IStringLocalizer<Resource>>())
        );
    }
}