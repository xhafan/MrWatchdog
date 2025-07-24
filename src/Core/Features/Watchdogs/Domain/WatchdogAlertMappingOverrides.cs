using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class WatchdogAlertMappingOverrides : IAutoMappingOverride<WatchdogAlert>
{
    public void Override(AutoMapping<WatchdogAlert> mapping)
    {
        mapping.HasMany(x => x.PreviousScrapedResults)
            .Table($"{nameof(WatchdogAlert)}PreviousScrapedResult")
            .Element("Value", x =>
            {
                x.Not.Nullable();
                x.Length(10000);
            });
        mapping.HasMany(x => x.CurrentScrapedResults)
            .Table($"{nameof(WatchdogAlert)}CurrentScrapedResult")
            .Element("Value", x =>
            {
                x.Not.Nullable();
                x.Length(10000);
            });        
    }
}