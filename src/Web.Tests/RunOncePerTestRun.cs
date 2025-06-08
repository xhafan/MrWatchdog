using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Web.Tests;

[SetUpFixture]
public class RunOncePerTestRun : BaseRunOncePerTestRun
{
    public static Lazy<WebApplicationFactory<Program>> WebApplicationFactory = null!;
    public static Lazy<HttpClient> WebApplicationClient = null!;
    
    [OneTimeSetUp]
    public void SetUp()
    {
        WebApplicationFactory = new Lazy<WebApplicationFactory<Program>>(() =>
        {
            return new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration((_, config) =>
                    {
                        var appSettingsTestPath = Path.Combine(AppContext.BaseDirectory, "appsettings.Test.json");
                        config.AddJsonFile(appSettingsTestPath);
                    });
                });
        });
        WebApplicationClient = new Lazy<HttpClient>(() => WebApplicationFactory.Value.CreateClient());
    }
    
    [OneTimeTearDown]
    public void TearDown()
    {
        if (WebApplicationClient.IsValueCreated)
        {
            WebApplicationClient.Value.Dispose();
        }
        if (WebApplicationFactory.IsValueCreated)
        {
            WebApplicationFactory.Value.Dispose();
        }
    }
}
