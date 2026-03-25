using Castle.MicroKernel.Registration;
using Castle.Windsor;
using CoreBackend.Register.Castle;
using CoreDdd.Domain.Repositories;
using CoreDdd.Queries;
using Microsoft.Playwright;
using MrWatchdog.Core.Features.Account.Queries;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Services;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Bus;
using Rebus.CastleWindsor;

namespace MrWatchdog.Web.HostedServices;

public static class RebusHostedServiceWindsorContainerRegistrator
{
    public static void RegisterServices(
        WindsorContainer hostedServiceWindsorContainer,
        IServiceProvider mainServiceProvider
    )
    {
        hostedServiceWindsorContainer.AddCoreBackendServicesFromMainServiceProvider(mainServiceProvider);

        var fireAndForgetWebBus = mainServiceProvider.GetRequiredService<IBus>();
        hostedServiceWindsorContainer.AddCoreDddAndCoreBackendServices(fireAndForgetWebBus);

        hostedServiceWindsorContainer.Register(
            Component.For<IPlaywright>()
                .Instance(mainServiceProvider.GetRequiredService<IPlaywright>())
        );

        hostedServiceWindsorContainer.Register(
            Classes
                .FromAssemblyContaining<UserRepository>()
                .BasedOn(typeof(IRepository<>))
                .WithService.AllInterfaces()
                .Configure(x => x.LifestyleTransient()),
            Classes
                .FromAssemblyContaining<GetUserByEmailQueryHandler>()
                .BasedOn(typeof(IQueryHandler<,>))
                .WithService.AllInterfaces()
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

        hostedServiceWindsorContainer.AutoRegisterHandlersFromAssemblyOf<CreateScraperCommandMessageHandler>();
    }
}
