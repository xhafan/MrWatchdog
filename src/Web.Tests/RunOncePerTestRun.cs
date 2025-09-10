using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Web.Tests;

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
                    builder.ConfigureAppConfiguration((_, config) =>
                    {
                        var appSettingsTestPath = Path.Combine(AppContext.BaseDirectory, "appsettings.Test.json");
                        config.AddJsonFile(appSettingsTestPath);
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
