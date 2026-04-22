using System.Data;
using System.Reflection;
using CoreBackend.Infrastructure;
using CoreBackend.Infrastructure.Configurations;
using CoreBackend.TestsShared.Loggers;
using CoreDdd.Domain.Events;
using CoreDdd.Nhibernate.Configurations;
using CoreDdd.TestHelpers.DomainEvents;
using CoreUtils;
using Microsoft.Extensions.Configuration;

namespace CoreBackend.TestsShared;

[SetUpFixture]
public abstract class BaseCoreBackendRunOncePerTestRun
{
    private const string EnvironmentName = "Test";

    protected abstract INhibernateConfigurator GetNhibernateConfigurator(string connectionString);
    protected abstract IDbConnection CreateConnection(string connectionString);
    protected virtual Assembly? GetAssemblyWithUserSecrets() => null;

    [OneTimeSetUp]
    public void BaseCoreBackendSetUp()
    {
        if (Environment.GetEnvironmentVariable(ConsoleAppSettings.AspNetCoreEnvironmentVariable) == null)
        {
            Environment.SetEnvironmentVariable(ConsoleAppSettings.AspNetCoreEnvironmentVariable, EnvironmentName);
        }

        ConsoleAppSettings.Initialize(GetAssemblyWithUserSecrets());

        var connectionStringName = ConsoleAppSettings.Configuration["DatabaseConnectionStringName"];
        Guard.Hope(connectionStringName != null, nameof(connectionStringName) + " is null");
        var connectionString = ConsoleAppSettings.Configuration.GetConnectionString(connectionStringName);
        Guard.Hope(connectionString != null, nameof(connectionString) + " is null");
        _BuildDatabase(() => CreateConnection(connectionString));

        TestFixtureContext.NhibernateConfigurator = GetNhibernateConfigurator(connectionString);
        
        var domainEventHandlerFactory = new FakeDomainEventHandlerFactory(
            domainEvent =>
            {
                Guard.Hope(TestFixtureContext.RaisedDomainEvents.Value != null, "TestFixtureContext.RaisedDomainEvents.Value" + " is null");
                TestFixtureContext.RaisedDomainEvents.Value.Add((IDomainEvent) domainEvent);
            });
        DomainEvents.Initialize(domainEventHandlerFactory);  
    }

    private void _BuildDatabase(Func<IDbConnection> createConnectionFunc)
    {
        var databaseScriptsDirectoryPath = ConsoleAppSettings.Configuration["DatabaseScriptsDirectoryPath"];
        Guard.Hope(databaseScriptsDirectoryPath != null, nameof(databaseScriptsDirectoryPath) + " is null");
        DatabaseBuilderHelper.BuildDatabase(
            createConnectionFunc,
            databaseScriptsDirectoryPath,
            new ConsoleTestLogger(),
            EnvironmentName
        );
    }
    
    [OneTimeTearDown]
    public void BaseCoreBackendTearDown()
    {
        (TestFixtureContext.NhibernateConfigurator as IDisposable)?.Dispose();
    }
}