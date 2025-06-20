﻿using Castle.MicroKernel.Registration;
using Castle.Windsor;
using CoreDdd.Domain.Events;
using CoreDdd.Queries;
using Microsoft.Extensions.Logging;
using MrWatchdog.Core.Features.Jobs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;

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
            Component.For<ICoreBus>().ImplementedBy<CoreBus>().LifeStyle.Singleton,
            Classes
                .FromAssemblyContaining(typeof(SendDomainEventOverMessageBusDomainEventHandler<>))
                .BasedOn(typeof(IDomainEventHandler<>))
                .WithService.FirstInterface()
                .Configure(x => x.LifestyleTransient())
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
                .Instance(mainWindsorContainer.Resolve<IHttpClientFactory>())
        );
    }
}