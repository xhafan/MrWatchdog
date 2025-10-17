using CoreDdd.Domain.Events;
using CoreDdd.TestHelpers.DomainEvents;
using CoreUtils;
using Microsoft.Extensions.Configuration;
using MrWatchdog.Core.Infrastructure;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.TestsShared.Loggers;

namespace MrWatchdog.TestsShared;

[SetUpFixture]
public class BaseRunOncePerTestRun
{
    private const string EnvironmentName = "Test";

    [OneTimeSetUp]
    public void BaseSetUp()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentName);
        
        var connectionStringName = ConsoleAppSettings.Configuration["DatabaseConnectionStringName"];
        Guard.Hope(connectionStringName != null, nameof(connectionStringName) + " is null");
        var connectionString = ConsoleAppSettings.Configuration.GetConnectionString(connectionStringName);
        Guard.Hope(connectionString != null, nameof(connectionString) + " is null");
        _BuildDatabase(connectionString);

        TestFixtureContext.NhibernateConfigurator = new NhibernateConfigurator(connectionString);
        
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
            new ConsoleTestLogger(),
            EnvironmentName
        );
    }
    
    [OneTimeTearDown]
    public void BaseTearDown()
    {
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        TestFixtureContext.NhibernateConfigurator?.Dispose();
    }
}