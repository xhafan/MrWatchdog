using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Web.E2E.Tests;

[SetUpFixture]
public class RunOncePerTestRun : BaseRunOncePerTestRun
{
    public static Lazy<WebApplicationFactory<Program>> WebApplicationFactory = null!;
    public static Lazy<HttpClient> SharedWebApplicationClient = null!;
    
    [OneTimeSetUp]
    public void SetUp()
    {
        WebApplicationFactory = new Lazy<WebApplicationFactory<Program>>(() =>
        {
            return new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.AddConsole();
                        logging.SetMinimumLevel(LogLevel.Trace);
                    });

                    builder.ConfigureAppConfiguration((_, config) =>
                    {
                        var appSettingsTestPath = Path.Combine(AppContext.BaseDirectory, "appsettings.Test.json");
                        config.AddJsonFile(appSettingsTestPath);
                        config.AddUserSecrets<Program>();
                        config.AddEnvironmentVariables();
                    });
                });
        });
        SharedWebApplicationClient = new Lazy<HttpClient>(() => WebApplicationFactory.Value.CreateDefaultClient());
    }
    
    [OneTimeTearDown]
    public void TearDown()
    {
        if (SharedWebApplicationClient.IsValueCreated)
        {
            SharedWebApplicationClient.Value.Dispose();
        }
        if (WebApplicationFactory.IsValueCreated)
        {
            WebApplicationFactory.Value.Dispose();
        }
    }
}