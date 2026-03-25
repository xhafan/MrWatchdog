using CoreBackend.Register.ServiceProvider;
using CoreDdd.Domain.Repositories;
using CoreDdd.Nhibernate.Configurations;
using CoreDdd.Queries;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using MrWatchdog.Core.Features.Account.Queries;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Services;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Bus;
using Rebus.Handlers;

namespace MrWatchdog.Web.HostedServices;

public static class RebusHostedServiceServiceProviderRegistrator
{
    public static IServiceProvider RegisterServices(IServiceProvider mainServiceProvider)
    {
        var hostedServiceServices = new ServiceCollection();

        hostedServiceServices.AddCoreBackendServicesFromMainServiceProvider(mainServiceProvider);
        
        var configuration = mainServiceProvider.GetRequiredService<IConfiguration>();
        var fireAndForgetWebBus = mainServiceProvider.GetRequiredService<IBus>();
        var hibernateConfigurator = mainServiceProvider.GetRequiredService<INhibernateConfigurator>();
        hostedServiceServices.AddCoreDddAndCoreBackendServices(configuration, fireAndForgetWebBus, hibernateConfigurator);

        hostedServiceServices.AddSingleton(mainServiceProvider.GetRequiredService<IPlaywright>());

        // register repositories from Core
        hostedServiceServices.Scan(scan => scan
            .FromAssemblyOf<UserRepository>()
            .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime()
        );
        // register query handlers from Core
        hostedServiceServices.Scan(scan => scan
            .FromAssemblyOf<GetUserByEmailQueryHandler>()
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime()
        );

        // register web scrapers
        hostedServiceServices.Scan(scan => scan
            .FromAssemblyOf<IWebScraper>()
            .AddClasses(classes => classes.AssignableTo<IWebScraper>())
            .AsImplementedInterfaces()
            .WithTransientLifetime()
        );
        hostedServiceServices.AddTransient<IWebScraperChain, WebScraperChain>();

        // register Rebus message handlers from Core
        hostedServiceServices.Scan(scan => scan
            .FromAssemblyOf<CreateScraperCommandMessageHandler>()
            .AddClasses(classes => classes.AssignableTo(typeof(IHandleMessages<>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime()
        );

        return hostedServiceServices.BuildServiceProvider();
    }
}
