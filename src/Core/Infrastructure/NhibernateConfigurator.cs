using System.Reflection;
using CoreDdd.Nhibernate.Configurations;
using Microsoft.Extensions.Configuration;
#if DEBUG
using HibernatingRhinos.Profiler.Appender.NHibernate;
#endif
using MrWatchdog.Core.Features.Watchdogs.Domain;

namespace MrWatchdog.Core.Infrastructure;

public class NhibernateConfigurator : BaseNhibernateConfigurator
{
    public NhibernateConfigurator(IConfiguration configuration) 
        : base(shouldMapDtos: false, connectionString: configuration.GetConnectionString("Database"))
    {
#if DEBUG
        NHibernateProfiler.Initialize();
#endif
    }

    protected override Assembly[] GetAssembliesToMap()
    {
        return
        [
            typeof(Watchdog).Assembly
        ];
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
#if DEBUG
            NHibernateProfiler.Shutdown();
#endif
        }
        base.Dispose(disposing);
    }
}
