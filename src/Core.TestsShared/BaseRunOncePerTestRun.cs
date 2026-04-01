using System.Data;
using System.Reflection;
using CoreBackend.Features.Jobs.Domain;
using CoreBackend.TestsShared;
using CoreDdd.Domain;
using CoreDdd.Nhibernate.Configurations;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure;
using Npgsql;

namespace MrWatchdog.Core.TestsShared;

[SetUpFixture]
public abstract class BaseRunOncePerTestRun : BaseCoreBackendRunOncePerTestRun
{
    [OneTimeSetUp]
    public void BaseSetUp()
    {
        TestFixtureContext.AggregateRootEntityTypes = _GetAggregateRootEntityTypes();
    }
    
    protected override INhibernateConfigurator GetNhibernateConfigurator(string connectionString)
    {
        return new NhibernateConfigurator(connectionString);
    }

    protected override IDbConnection CreateConnection(string connectionString)
    {
        return new NpgsqlConnection(connectionString);
    }

    protected override Assembly GetAssemblyWithUserSecrets() => typeof(Scraper).Assembly;

    private static ICollection<Type> _GetAggregateRootEntityTypes()
    {
        var assemblies = new[]{
            typeof(Job).Assembly,
            typeof(Scraper).Assembly,
        };

        return assemblies.SelectMany(x => x.GetTypes())
            .Where(x => typeof(IAggregateRoot).IsAssignableFrom(x)
                        && !x.IsAbstract
                        && x.IsPublic)
            .ToList();
    }
}
