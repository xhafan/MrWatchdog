using Castle.MicroKernel.Registration;
using Castle.Windsor;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.Web.Infrastructure;

public static class WindsorContainerRegistrator
{
    public static void RegisterCommonServices(IWindsorContainer windsorContainer)
    {
        windsorContainer.Register(
            Component.For<IRepository<Watchdog>>().ImplementedBy<NhibernateRepository<Watchdog>>().LifeStyle.Transient
        );
    }

    public static void RegisterServicesFromMainWindsorContainer(IWindsorContainer windsorContainer, IWindsorContainer mainWindsorContainer)
    {
        windsorContainer.Register(
            Component.For<ILoggerFactory>()
                .Instance(mainWindsorContainer.Resolve<ILoggerFactory>()),
            Component.For(typeof(ILogger<>))
                .UsingFactoryMethod((_, creationContext) => mainWindsorContainer.Resolve(typeof(ILogger<>).MakeGenericType(creationContext.GenericArguments[0])))
        );
    }
}