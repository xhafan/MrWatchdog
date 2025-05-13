using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.MsDependencyInjection;
using CoreDdd.Nhibernate.Configurations;
using MrWatchdog.Core.Infrastructure;
using MrWatchdog.Core.Messages;
using MrWatchdog.Web;
using MrWatchdog.Web.Infrastructure;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Rebus.Transport.InMem;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var mvcBuilder = builder.Services.AddRazorPages();

if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
}

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHsts(options =>
    {
        options.MaxAge = TimeSpan.FromDays(365);
    });
}

var rebusInMemoryNetwork = new InMemNetwork();
builder.Services.AddSingleton(rebusInMemoryNetwork);

var webEnvironmentInputQueueName = $"{builder.Environment.EnvironmentName}Web";
var mainRebusHostedServiceEnvironmentInputQueueName = $"{builder.Environment.EnvironmentName}Main";

builder.Services.AddRebus((configure, serviceProvider) => configure
    .Logging(x => x.MicrosoftExtensionsLogging(serviceProvider.GetRequiredService<ILoggerFactory>()))
    .Transport(x => x.UseInMemoryTransport(rebusInMemoryNetwork, webEnvironmentInputQueueName, registerSubscriptionStorage: false))
    .Routing(x => x.TypeBased().MapAssemblyDerivedFrom<Command>(mainRebusHostedServiceEnvironmentInputQueueName))
    .Options(x =>
    {
        x.SetNumberOfWorkers(0); // no worker for unused Web queue
        x.SetMaxParallelism(1); // must be 1 to make Rebus happy
    })
);

builder.Host.UseServiceProviderFactory(new WindsorServiceProviderFactory());

builder.Host.ConfigureContainer<IWindsorContainer>(mainWindsorContainer =>
{
    mainWindsorContainer.Register(
        Component.For<INhibernateConfigurator>()
            .ImplementedBy<NhibernateConfigurator>()
            .DependsOn(new {connectionString = 
                "Host=localhost;Database=mr_watchdog;Username=mr_watchdog;Password=Password01;Include Error Detail=true;"
            })
            .LifeStyle.Singleton
    );

    WindsorContainerRegistrator.RegisterServices(mainWindsorContainer, setUnitOfWorkLifeStyleFunc: x => x.Scoped());
});

builder.Services.AddSingleton<IHostedService>(serviceProvider =>
    new RebusHostedService(
        mainRebusHostedServiceEnvironmentInputQueueName,
        serviceProvider.GetRequiredService<InMemNetwork>(),
        serviceProvider.GetRequiredService<INhibernateConfigurator>(),
        serviceProvider.GetRequiredService<ILoggerFactory>()
    )
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

var mainWindsorContainer = app.Services.GetRequiredService<IWindsorContainer>();
mainWindsorContainer.Dispose();
