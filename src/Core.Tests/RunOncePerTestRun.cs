using CoreDdd.Domain.Events;
using CoreDdd.TestHelpers.DomainEvents;
using CoreUtils;
using Microsoft.Extensions.Configuration;
using MrWatchdog.Core.Configurations;
using MrWatchdog.Core.Infrastructure;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Loggers;

namespace MrWatchdog.Core.Tests;

[SetUpFixture]
public class RunOncePerTestRun
{
    [OneTimeSetUp]
    public void SetUp()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
        
        var connectionString = ConsoleAppSettings.Configuration.GetConnectionString("Database");
        Guard.Hope(connectionString != null, nameof(connectionString) + " is null");
        _BuildDatabase(connectionString);

        TestFixtureContext.NhibernateConfigurator = new NhibernateConfigurator(ConsoleAppSettings.Configuration);
        
        var domainEventHandlerFactory = new FakeDomainEventHandlerFactory(
            domainEvent =>
            {
                Guard.Hope(TestFixtureContext.RaisedDomainEvents.Value != null, "TestFixtureContext.RaisedDomainEvents.Value" + " is null");
                TestFixtureContext.RaisedDomainEvents.Value.Add((IDomainEvent) domainEvent);
            });
        DomainEvents.Initialize(domainEventHandlerFactory);        
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
        TestFixtureContext.NhibernateConfigurator?.Dispose();
    }
}
