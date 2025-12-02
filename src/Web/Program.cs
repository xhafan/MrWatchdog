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
using MrWatchdog.Core.Infrastructure.Rebus.MessageRouting;
using MrWatchdog.Core.Infrastructure.Rebus.RebusQueueRedirectors;
using MrWatchdog.Core.Infrastructure.RequestIdAccessors;
using MrWatchdog.Core.Resources;
using MrWatchdog.Web.Features.Account.Login;
using MrWatchdog.Web.Features.Logs;
using MrWatchdog.Web.HostedServices;
using MrWatchdog.Web.Infrastructure.ActingUserAccessors;
using MrWatchdog.Web.Infrastructure.Authorizations;
using MrWatchdog.Web.Infrastructure.PageFilters;
using MrWatchdog.Web.Infrastructure.RateLimiting;
using MrWatchdog.Web.Infrastructure.Rebus.RebusQueueRedirectors;
using MrWatchdog.Web.Infrastructure.RequestIdAccessors;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using Rebus.Config;
using Rebus.Retry.Simple;
using Rebus.Transport.InMem;
using Serilog;
using Serilog.Sinks.PostgreSQL;
using System.Data;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.RateLimiting;
using CoreUtils;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using MrWatchdog.Web.Infrastructure.Middlewares;
using MrWatchdog.Web.Infrastructure.Validations;

namespace MrWatchdog.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // enable support for "windows-1250" and other legacy encodings to make HttpClient.ReadAsStringAsync() work correctly
        
        var builder = WebApplication.CreateBuilder(args);

        var environmentName = builder.Environment.EnvironmentName;
        if (builder.Environment.IsDevelopment() 
            || environmentName == "LocalStaging"
            || environmentName == "Test")
        {
            builder.Configuration.AddUserSecrets<Program>();
        }
       
        var connectionStringName = builder.Configuration["DatabaseConnectionStringName"];
        Guard.Hope(connectionStringName != null, nameof(connectionStringName) + " is null");
        var connectionString = builder.Configuration.GetConnectionString(connectionStringName);
        Guard.Hope(connectionString != null, nameof(connectionString) + " is null");

        _configureLogging();

        Log.Information("Starting {MrWatchdog}, environment {ENVIRONMENT}, PID {PID}", 
            Resource.MrWatchdog,
            environmentName,
            Environment.ProcessId
        );

        // Add services to the container.

        // Castle Windsor is the root container, not the default .NET DI. Rebus hosted services use Castle Windsor as the container
        // to be able to use advanced features like interceptors, etc., and there are common services registrations for Castle Windsor like query handlers, etc.
        builder.Host.UseServiceProviderFactory(new WindsorServiceProviderFactory());
        builder.Host.ConfigureContainer<IWindsorContainer>(windsorContainer =>
            WindsorContainerRegistrator.RegisterCommonServices(windsorContainer, builder.Configuration)
        );

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

        builder.Services.AddHealthChecks();

        if (builder.Environment.IsDevelopment())
        {
            mvcBuilder.AddRazorRuntimeCompilation();
        }

        builder.Services.AddCoreDdd();
        builder.Services.AddCoreDddNhibernate<NhibernateConfigurator>(_ => new NhibernateConfigurator(connectionString));
        builder.Services.AddSingleton<IJobCreator, NewTransactionJobCreator>();
        builder.Services.AddSingleton<ICoreBus, CoreBus>();
        

        InMemNetwork? rebusInMemoryNetwork = null;
        if (builder.Configuration["Rebus:Transport"] == "InMemory")
        {
            rebusInMemoryNetwork = new InMemNetwork();
            builder.Services.AddSingleton(rebusInMemoryNetwork);
        }
        
        _addRebusForSendingOnly();

        foreach (var queue in RebusQueues.AllQueues.Value)
        {
            var inputQueueName = $"{queue}";

            builder.Services.AddSingleton<IHostedService>(serviceProvider =>
                new RebusHostedService(
                    inputQueueName,
                    environmentName,
                    serviceProvider.GetRequiredService<INhibernateConfigurator>(),
                    serviceProvider.GetRequiredService<IWindsorContainer>(),
                    serviceProvider.GetRequiredService<IConfiguration>(),
                    connectionString
                )
            );

            var inputQueueNameForSending = $"{inputQueueName}{RebusConstants.RebusSendQueueSuffix}";

            builder.Services.AddSingleton<IHostedService>(serviceProvider =>
                new RebusHostedService(
                    inputQueueNameForSending,
                    environmentName,
                    serviceProvider.GetRequiredService<INhibernateConfigurator>(),
                    serviceProvider.GetRequiredService<IWindsorContainer>(),
                    serviceProvider.GetRequiredService<IConfiguration>(),
                    connectionString
                )
            );
        }

        builder.Services.Configure<KickOffDueWatchdogsScrapingHostedServiceOptions>(
            builder.Configuration.GetSection(nameof(KickOffDueWatchdogsScrapingHostedService))
        );
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

        builder.Services.AddSingleton<IValidationAttributeAdapterProvider, LocalizedValidationAttributeAdapterProvider>();        
        builder.Services.AddLocalization();

        var supportedCultures = new[] { "cs", "en" };

        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            options.SetDefaultCulture("en");
            options.AddSupportedCultures(supportedCultures);
            options.AddSupportedUICultures(supportedCultures);

            // use "Accept-Language" header
            options.RequestCultureProviders.Insert(0, new AcceptLanguageHeaderRequestCultureProvider());
        });
       
        builder.Services.Configure<SmtpServerEmailSenderOptions>(builder.Configuration.GetSection(nameof(SmtpServerEmailSender)));
        builder.Services.Configure<SmtpClientDirectlyToRecipientMailServerEmailSenderOptions>(
            builder.Configuration.GetSection(nameof(SmtpClientDirectlyToRecipientMailServerEmailSender))
        );
        builder.Services.Configure<EmailAddressesOptions>(builder.Configuration.GetSection("EmailAddresses"));
        
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
            })
            .AddGoogle(googleOptions =>
            {
                var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
                var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

                Guard.Hope(googleClientId != null, nameof(googleClientId) + " is null");
                Guard.Hope(googleClientSecret != null, nameof(googleClientSecret) + " is null");

                googleOptions.ClientId = googleClientId;
                googleOptions.ClientSecret = googleClientSecret;

                googleOptions.Events.OnRemoteFailure = context =>
                {
                    var redirectUrl = context.Properties?.Items.TryGetValue(AccountUrlConstants.ReturnUrl, out var returnUrl) == true
                        ? QueryHelpers.AddQueryString(
                            AccountUrlConstants.AccountLoginUrl,
                            new Dictionary<string, string?> {{AccountUrlConstants.ReturnUrl, returnUrl}}
                        )
                        : AccountUrlConstants.AccountLoginUrl;
                    context.Response.Redirect(redirectUrl);
                    context.HandleResponse();
                    return Task.CompletedTask;
                };
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
        builder.Services.AddSingleton<IRebusQueueRedirector, HttpContextRebusQueueRedirector>();
        
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

        var rateLimitingOptions = OptionsRetriever.Retrieve<RateLimitingOptions>(builder.Configuration, builder.Services);
        builder.Services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext => _getSlidingWindowLimiter(
                httpContext,
                rateLimitingOptions.GlobalRequestsPerSecondPerUserPermitLimit,
                rateLimitingOptions.GlobalRequestsPerSecondPerUserQueueLimit,
                TimeSpan.FromSeconds(1)
            ));

            options.AddPolicy(RateLimitingConstants.LogErrorsRequestsPerSecondPerUserPolicy, httpContext => _getSlidingWindowLimiter(
                httpContext,
                rateLimitingOptions.LogErrorRequestsPerSecondPerUserPermitLimit,
                rateLimitingOptions.LogErrorRequestsPerSecondPerUserQueueLimit,
                TimeSpan.FromSeconds(1)
            ));

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken);
            };
            return;

            RateLimitPartition<string> _getSlidingWindowLimiter(
                HttpContext httpContext,
                int permitLimit,
                int queueLimit,
                TimeSpan window
                )
            {
                var userIdOrClientIpAddressPartitionKey = HttpContextUserIdOrClientIpAddressGetter.GetUserIdOrClientIpAddress(httpContext);

                return RateLimitPartition.GetSlidingWindowLimiter(userIdOrClientIpAddressPartitionKey, _ =>
                    new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit,
                        QueueLimit = queueLimit,
                        SegmentsPerWindow = 20, // ChatGPT: For most cases, SegmentsPerWindow = 10-20 offers a good balance between performance and accuracy. (https://chatgpt.com/share/67351dd5-0f88-8000-b49d-ea315ce2ab3c)
                        Window = window
                    });
            }
        });

        builder.Services.Configure<LoggingOptions>(builder.Configuration.GetSection(LogConstants.LoggingConfigurationSectionName));


        var app = builder.Build();
        var mainWindsorContainer = app.Services.GetRequiredService<IWindsorContainer>();

        _buildDatabase();

        // Configure the HTTP request pipeline.
            
        var forwardedHeadersOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor // identify client IP address
                               | ForwardedHeaders.XForwardedProto // to make login via Google use https
                               | ForwardedHeaders.XForwardedHost,
            ForwardLimit = null
        };
        forwardedHeadersOptions.KnownNetworks.Clear();
        forwardedHeadersOptions.KnownProxies.Clear();

        app.UseForwardedHeaders(forwardedHeadersOptions);

        app.UseResponseCompression();
        
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

        var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
        app.UseRequestLocalization(localizationOptions);

        app.UseRouting();
        app.UseHealthChecks("/up", new HealthCheckOptions
        {
            ResponseWriter = async (httpContext, _) =>
            {
                httpContext.Response.ContentType = "application/json";

                await httpContext.Response.WriteAsync(JsonHelper.Serialize(new
                {
                    Status = "Healthy",
                    Environment = environmentName,
                    Host = Environment.GetEnvironmentVariable("KAMAL_HOST"),
                    Version = Environment.GetEnvironmentVariable("KAMAL_VERSION")
                }));
            }
        });

        app.UseAuthentication();
        app.UseMiddleware<SerilogRequestIdEnricherMiddleware>();
        app.UseMiddleware<SerilogUserEnricherMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseAuthorization();
        app.UseRateLimiter();
        
        app.UseMiddleware<HandleOldAssetRequestsMiddleware>();
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

        void _addRebusForSendingOnly()
        {
            var webEnvironmentInputQueueName = $"{environmentName}Web";

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
                                connectionString,
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
                    .Routing(configurer => MessageRoutingConfigurator.ConfigureMessageRouting(configurer, environmentName))
                    .Options(x =>
                    {
                        x.RetryStrategy(errorQueueName: $"{webEnvironmentInputQueueName}Error");
                        x.SetNumberOfWorkers(0); // no worker for unused Web queue
                        x.SetMaxParallelism(1); // must be 1 to make Rebus happy
                    });
            });
        }

        void _buildDatabase()
        {
            var logger = mainWindsorContainer.Resolve<ILogger<BuilderOfDatabase>>();
            var configuration = mainWindsorContainer.Resolve<IConfiguration>();
            var connectionStringWithTimeout = $"{connectionString}CommandTimeout=120;";
            var databaseScriptsDirectoryPath = configuration["DatabaseScriptsDirectoryPath"]!;
            DatabaseBuilderHelper.BuildDatabase(connectionStringWithTimeout, databaseScriptsDirectoryPath, logger, environmentName);
        }

        void _configureLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.PostgreSQL(
                    connectionString: connectionString,
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