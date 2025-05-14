using CoreUtils;
using Microsoft.Extensions.Configuration;
using MrWatchdog.Core.Configurations;
using MrWatchdog.Core.Infrastructure;
using MrWatchdog.TestsShared.Loggers;

namespace MrWatchdog.Core.Tests;

[SetUpFixture]
public class RunOncePerTestRun
{
    public static NhibernateConfigurator? NhibernateConfigurator;

    [OneTimeSetUp]
    public void SetUp()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
        
        var connectionString = ConsoleAppSettings.Configuration.GetConnectionString("Database");
        Guard.Hope(connectionString != null, nameof(connectionString) + " is null");
        _BuildDatabase(connectionString);

        NhibernateConfigurator = new NhibernateConfigurator(ConsoleAppSettings.Configuration);
    }

    private void _BuildDatabase(string connectionString)
    {
        var databaseScriptsDirectoryPath = ConsoleAppSettings.Configuration["DatabaseScriptsDirectoryPath"];
        Guard.Hope(databaseScriptsDirectoryPath != null, nameof(databaseScriptsDirectoryPath) + " is null");
        DatabaseBuilderHelper.BuildDatabase(
            connectionString,
            databaseScriptsDirectoryPath,
            new TestLogger()
        );
    }
    
    [OneTimeTearDown]
    public void TearDown()
    {
        NhibernateConfigurator?.Dispose();
    }
}
