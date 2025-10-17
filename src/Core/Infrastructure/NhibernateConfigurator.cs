using System.Reflection;
using CoreDdd.Nhibernate.Configurations;
#if DEBUG
using HibernatingRhinos.Profiler.Appender.NHibernate;
#endif
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Conventions;
using MrWatchdog.Core.Infrastructure.Interceptors;
using NHibernate.Cfg;

namespace MrWatchdog.Core.Infrastructure;

public class NhibernateConfigurator : BaseNhibernateConfigurator
{
    public NhibernateConfigurator(string connectionString) 
        : base(shouldMapDtos: false, connectionString: connectionString)
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
    
    protected override string GetIdentityHiLoMaxLo()
    {
        // Generated Id = (maxlo + 1) * hi; the lower the maxLo value, the fewer the gaps in Ids when a new app version is deployed.
        return "99"; // this gives Id range of 100 (99 + 1) Ids per hi value.
    }    
    
    protected override IEnumerable<Type> GetAdditionalConventions()
    {
        yield return typeof(EnumConvention);
        yield return typeof(DateTimeConvention);
    } 

    protected override void AdditionalConfiguration(Configuration configuration)
    {
        configuration.SetInterceptor(new CompositeInterceptor([
            new EntityJobTrackingInterceptor()
        ]));
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
