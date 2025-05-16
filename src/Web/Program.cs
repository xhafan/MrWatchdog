using System.Data;
using System.Text.RegularExpressions;
using Castle.Windsor;
using Castle.Windsor.MsDependencyInjection;
using CoreDdd.AspNetCore.Middlewares;
using CoreDdd.Nhibernate.Configurations;
using CoreDdd.Nhibernate.Register.DependencyInjection;
using CoreDdd.Register.DependencyInjection;
using DatabaseBuilder;
using MrWatchdog.Core.Infrastructure;
using MrWatchdog.Core.Messages;
using MrWatchdog.Web;
using MrWatchdog.Web.Infrastructure;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Rebus.Transport.InMem;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Castle Windsor is the root container, not the default .NET DI. Rebus hosted services use Castle Windsor as the container
// to be able to use advanced features like interceptors, etc., and there are common services registrations for Castle Windsor like query handlers, etc.
builder.Host.UseServiceProviderFactory(new WindsorServiceProviderFactory());
builder.Host.ConfigureContainer<IWindsorContainer>(WindsorContainerRegistrator.RegisterCommonServices);

var mvcBuilder = builder.Services.AddRazorPages();

if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
}

var rebusInMemoryNetwork = new InMemNetwork();
builder.Services.AddSingleton(rebusInMemoryNetwork);

builder.Services.AddCoreDdd();
builder.Services.AddCoreDddNhibernate<NhibernateConfigurator>();

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

builder.Services.AddSingleton<IHostedService>(serviceProvider =>
    new RebusHostedService(
        mainRebusHostedServiceEnvironmentInputQueueName,
        serviceProvider.GetRequiredService<InMemNetwork>(),
        serviceProvider.GetRequiredService<INhibernateConfigurator>(),
        serviceProvider.GetRequiredService<ILoggerFactory>()
    )
);

var app = builder.Build();

_buildDatabase(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

var getOrHeadRequestPathsWithoutDefaultDatabaseTransaction = new List<Regex>
{
    new("^/assets/.*", RegexOptions.IgnoreCase),
    new("^/lib/.*", RegexOptions.IgnoreCase),
    new(@"^/favicon\.ico$", RegexOptions.IgnoreCase)
};

app.UseMiddleware<UnitOfWorkDependencyInjectionMiddleware>(
    IsolationLevel.ReadCommitted,
    getOrHeadRequestPathsWithoutDefaultDatabaseTransaction
);

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

await app.RunAsync();

// Castle Windsor is the root container and it needs to be disposed manually.
var mainWindsorContainer = app.Services.GetRequiredService<IWindsorContainer>();
mainWindsorContainer.Dispose();


void _buildDatabase(IServiceProvider services)
{
    var logger = services.GetRequiredService<ILogger<BuilderOfDatabase>>();
    var configuration = services.GetRequiredService<IConfiguration>();
    var connectionString = $"{configuration.GetConnectionString("Database")}CommandTimeout=120;";
    var databaseScriptsDirectoryPath = configuration["DatabaseScriptsDirectoryPath"]!;
    DatabaseBuilderHelper.BuildDatabase(connectionString, databaseScriptsDirectoryPath, logger);
}