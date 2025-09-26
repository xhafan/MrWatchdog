using AspNetCore.ReCaptcha;
using Castle.Windsor;
using Castle.Windsor.MsDependencyInjection;
using CoreDdd.AspNetCore.Middlewares;
using CoreDdd.Domain.Events;
using CoreDdd.Nhibernate.Configurations;
using CoreDdd.Nhibernate.Register.DependencyInjection;
using CoreDdd.Register.DependencyInjection;
using DatabaseBuilder;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.OpenApi.Models;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Jobs.Services;
using MrWatchdog.Core.Infrastructure;
using MrWatchdog.Core.Infrastructure.ActingUserAccessors;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.RequestIdAccessors;
using MrWatchdog.Core.Messages;
using MrWatchdog.Core.Resources;
using MrWatchdog.Web.Features.Account.Login;
using MrWatchdog.Web.HostedServices;
using MrWatchdog.Web.Infrastructure.ActingUserAccessors;
using MrWatchdog.Web.Infrastructure.Authorizations;
using MrWatchdog.Web.Infrastructure.PageFilters;
using MrWatchdog.Web.Infrastructure.RequestIdAccessors;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Rebus.Config;
using Rebus.Retry.Simple;
using Rebus.Routing.TypeBased;
using Rebus.Transport.InMem;
using Serilog;
using Serilog.Sinks.PostgreSQL;
using System.Data;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace MrWatchdog.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // enable support for "windows-1250" and other legacy encodings to make HttpClient.ReadAsStringAsync() work correctly
        
        var builder = WebApplication.CreateBuilder(args);

        _configureLogging();

        if (builder.Environment.IsDevelopment() || builder.Environment.EnvironmentName == "LocalStaging")
        {
            builder.Configuration.AddUserSecrets<Program>();
        }
        
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
                options.Conventions.AuthorizeFolder("/");
                options.Conventions.ConfigureFilter(new EnforceHandlerExistsPageFilter());
            });
        builder.Services.AddControllers(options =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            options.Filters.Add(new AuthorizeFilter(policy));
        });

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
        
        builder.Services.Configure<KickOffDueWatchdogsScrapingHostedServiceOptions>(builder.Configuration.GetSection(nameof(KickOffDueWatchdogsScrapingHostedService)));
        builder.Services.AddHostedService<KickOffDueWatchdogsScrapingHostedService>();

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo {Title = $"{Resource.MrWatchdog} API", Version = "v1"});
        });
        
        // retry policy taken from https://dev.to/rickystam/net-core-use-httpclientfactory-and-polly-to-build-rock-solid-services-2edh
        // more info about timeouts: https://github.com/App-vNext/Polly/wiki/Polly-and-HttpClientFactory#use-case-applying-timeouts
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError() // HttpRequestException, 5XX and 408
            .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests) // 429
            .Or<TimeoutRejectedException>() // thrown by Polly's TimeoutPolicy if the inner call times out
            .WaitAndRetryAsync(
                HttpClientConstants.RetryCount,
                retryAttempt => TimeSpan.FromSeconds(HttpClientConstants.RetrySleepDurationInSecondsProvider(retryAttempt))
            );

        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(30); // Timeout for an individual try

        builder.Services.AddHttpClient(HttpClientConstants.HttpClientWithRetries)
            .AddPolicyHandler(retryPolicy)
            .AddPolicyHandler(timeoutPolicy); // the timeoutPolicy is inside the retryPolicy, to make it time out each try.
        
        builder.Services.AddHttpClient();
        builder.Services.AddLocalization();
       
        builder.Services.Configure<EmailSenderOptions>(builder.Configuration.GetSection(nameof(EmailSender)));
        
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = redirectContext =>
                    {
                        if (_isApiOrPostRequest(redirectContext))
                        {
                            redirectContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        }
                        else
                        {
                            redirectContext.Response.Redirect(new Uri(redirectContext.RedirectUri).PathAndQuery);
                        }

                        return Task.CompletedTask;
                    },
                    OnRedirectToAccessDenied = redirectContext =>
                    {
                        if (_isApiOrPostRequest(redirectContext))
                        {
                            redirectContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                        }
                        else
                        {
                            redirectContext.Response.Redirect(new Uri(redirectContext.RedirectUri).PathAndQuery);
                        }
                    
                        return Task.CompletedTask;
                    }
                };
                return;

                bool _isApiOrPostRequest(RedirectContext<CookieAuthenticationOptions> redirectContext)
                {
                    return redirectContext.Request.Path.StartsWithSegments("/api") 
                           || redirectContext.Request.Method == HttpMethods.Post;
                }
            });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.SuperAdmin, policy => policy.Requirements.Add(new SuperAdminRequirement()));
        });

        builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
        builder.Services.Configure<RuntimeOptions>(builder.Configuration.GetSection("Runtime"));
        
        builder.Services.AddSingleton<IJobCompletionAwaiter, JobCompletionAwaiter>();
        
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<IActingUserAccessor, HttpContextActingUserAccessor>();
        builder.Services.AddSingleton<IRequestIdAccessor, HttpContextRequestIdAccessor>();
        
        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });

        var reCaptchaConfigurationSection = builder.Configuration.GetSection("ReCaptcha");
        builder.Services.AddReCaptcha(reCaptchaConfigurationSection);        
        builder.Services.Configure<ReCaptchaOptions>(reCaptchaConfigurationSection);
        
        // register authorization handlers
        builder.Services.Scan(scan => scan

            .FromAssemblyOf<Program>()
            .AddClasses(classes => classes.AssignableTo(typeof(IAuthorizationHandler)))
            .AsImplementedInterfaces()
            .WithScopedLifetime()
        );

        var app = builder.Build();
        var mainWindsorContainer = app.Services.GetRequiredService<IWindsorContainer>();

        _buildDatabase();

        app.UseResponseCompression();
        
        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(context =>
                {
                    var requestId = context.TraceIdentifier;
                    context.Response.Redirect($"/Error?requestId={requestId}");
                    return Task.CompletedTask;
                });
            });            
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

        app.UseAuthentication();
        app.UseMiddleware<SerilogRequestIdEnricherMiddleware>();
        app.UseMiddleware<SerilogUserEnricherMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapRazorPages()
            .WithStaticAssets();
        app.MapControllers();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("v1/swagger.json", $"{Resource.MrWatchdog} API V1");
        });

        DomainEvents.Initialize(new DomainEventHandlerFactory());


        try
        {
            await app.RunAsync();
        }
        finally
        {
            // Castle Windsor is the root container and it needs to be disposed manually.
            mainWindsorContainer.Dispose();
        
            await Log.CloseAndFlushAsync();
        }
        return;

        void _buildDatabase()
        {
            var logger = mainWindsorContainer.Resolve<ILogger<BuilderOfDatabase>>();
            var configuration = mainWindsorContainer.Resolve<IConfiguration>();
            var connectionString = $"{configuration.GetConnectionString("Database")}CommandTimeout=120;";
            var databaseScriptsDirectoryPath = configuration["DatabaseScriptsDirectoryPath"]!;
            DatabaseBuilderHelper.BuildDatabase(connectionString, databaseScriptsDirectoryPath, logger);
        }

        void _configureLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.PostgreSQL(
                    connectionString: builder.Configuration.GetConnectionString("Database"),
                    tableName: """
                               "Logs"
                               """,
                    needAutoCreateTable: true,
                    columnOptions: new Dictionary<string, ColumnWriterBase>
                    {
                        {"""
                         "TimeStamp"
                         """, new TimestampColumnWriter()},
                        {"""
                         "Message"
                         """, new RenderedMessageColumnWriter()},
                        {"""
                         "MessageTemplate"
                         """, new MessageTemplateColumnWriter()},
                        {"""
                         "Level"
                         """, new LevelColumnWriter()},
                        {"""
                         "Exception"
                         """, new ExceptionColumnWriter()},
                        {"""
                         "Properties"
                         """, new PropertiesColumnWriter()}
                    },
                    formatProvider: null
                )
                .CreateLogger();

            builder.Host.UseSerilog();
        }
    }
}