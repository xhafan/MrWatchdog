using System.Data;
using Castle.Windsor;
using Castle.Windsor.MsDependencyInjection;
using CoreDdd.AspNetCore.Middlewares;
using CoreDdd.Domain.Events;
using CoreDdd.Nhibernate.Configurations;
using CoreDdd.Nhibernate.Register.DependencyInjection;
using CoreDdd.Register.DependencyInjection;
using DatabaseBuilder;
using Microsoft.OpenApi.Models;
using MrWatchdog.Core.Infrastructure;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Messages;
using Rebus.Config;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;
using System.Text.RegularExpressions;
using Rebus.Transport.InMem;

namespace MrWatchdog.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.AddSimpleConsole(options =>
        {
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
        });
        
        // Add services to the container.

        // Castle Windsor is the root container, not the default .NET DI. Rebus hosted services use Castle Windsor as the container
        // to be able to use advanced features like interceptors, etc., and there are common services registrations for Castle Windsor like query handlers, etc.
        builder.Host.UseServiceProviderFactory(new WindsorServiceProviderFactory());
        builder.Host.ConfigureContainer<IWindsorContainer>(WindsorContainerRegistrator.RegisterCommonServices);

        var mvcBuilder = builder.Services
            .AddRazorPages()
            .AddRazorPagesOptions(options =>
            {
                options.RootDirectory = "/Features";
                options.Conventions.Add(new PageRouteModelConvention());
            });
        builder.Services.AddControllers();

        if (builder.Environment.IsDevelopment())
        {
            mvcBuilder.AddRazorRuntimeCompilation();
        }

        builder.Services.AddCoreDdd();
        builder.Services.AddCoreDddNhibernate<NhibernateConfigurator>();
        builder.Services.AddSingleton<IJobCreator, NewTransactionJobCreator>();
        builder.Services.AddSingleton<ICoreBus, CoreBus>();
        
        var webEnvironmentInputQueueName = $"{builder.Environment.EnvironmentName}Web";
        var mainRebusHostedServiceEnvironmentInputQueueName = $"{builder.Environment.EnvironmentName}Main";

        InMemNetwork? rebusInMemoryNetwork = null;
        if (builder.Configuration["Rebus:Transport"] == "InMemory")
        {
            rebusInMemoryNetwork = new InMemNetwork();
            builder.Services.AddSingleton(rebusInMemoryNetwork);
        }
        
        builder.Services.AddRebus((configure, serviceProvider) =>
        {
            var rebusConfigurer = configure
                .Logging(x => x.MicrosoftExtensionsLogging(serviceProvider.GetRequiredService<ILoggerFactory>()));

            switch (builder.Configuration["Rebus:Transport"])
            {
                case "RabbitMq":
                    rebusConfigurer.Transport(x => x.UseRabbitMq("amqp://guest:guest@localhost", webEnvironmentInputQueueName));
                    break;
                
                case "PostgreSql":
                    rebusConfigurer
                        .Transport(x => x.UsePostgreSql(
                            builder.Configuration.GetConnectionString("Database"),
                            "RebusQueue",
                            webEnvironmentInputQueueName
                        ));
                    break;
                
                case "InMemory":
                    rebusConfigurer.Transport(x => x.UseInMemoryTransport(rebusInMemoryNetwork, webEnvironmentInputQueueName, registerSubscriptionStorage: false));
                    break;
                
                default:
                    throw new InvalidOperationException("Rebus transport is not configured. Set Rebus:Transport to RabbitMq, PostgreSql, or InMemory in appsettings.json.");
            }

            return rebusConfigurer
                .Routing(x => x.TypeBased().MapAssemblyDerivedFrom<Command>(mainRebusHostedServiceEnvironmentInputQueueName))
                .Options(x =>
                {
                    x.RetryStrategy(errorQueueName: $"{webEnvironmentInputQueueName}Error");
                    x.SetNumberOfWorkers(0); // no worker for unused Web queue
                    x.SetMaxParallelism(1); // must be 1 to make Rebus happy
                });
        });

        builder.Services.AddSingleton<IHostedService>(serviceProvider =>
            new RebusHostedService(
                mainRebusHostedServiceEnvironmentInputQueueName,
                serviceProvider.GetRequiredService<INhibernateConfigurator>(),
                serviceProvider.GetRequiredService<IWindsorContainer>(),
                serviceProvider.GetRequiredService<IConfiguration>()
            )
        );

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo {Title = "Mr Watchdog API", Version = "v1"});
        });
        
        builder.Services.AddHttpClient();

        var app = builder.Build();
        var mainWindsorContainer = app.Services.GetRequiredService<IWindsorContainer>();

        _buildDatabase();

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
        app.MapControllers();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("v1/swagger.json", "Mr Watchdog API V1");
        });

        DomainEvents.Initialize(new DomainEventHandlerFactory());

        await app.RunAsync();

        // Castle Windsor is the root container and it needs to be disposed manually.
        mainWindsorContainer.Dispose();
        return;

        void _buildDatabase()
        {
            var logger = mainWindsorContainer.Resolve<ILogger<BuilderOfDatabase>>();
            var configuration = mainWindsorContainer.Resolve<IConfiguration>();
            var connectionString = $"{configuration.GetConnectionString("Database")}CommandTimeout=120;";
            var databaseScriptsDirectoryPath = configuration["DatabaseScriptsDirectoryPath"]!;
            DatabaseBuilderHelper.BuildDatabase(connectionString, databaseScriptsDirectoryPath, logger);
        }
    }
}