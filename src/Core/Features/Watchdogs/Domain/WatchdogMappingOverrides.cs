using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class WatchdogMappingOverrides : IAutoMappingOverride<Watchdog>
{
    public void Override(AutoMapping<Watchdog> mapping)
    {
        mapping.HasMany(x => x.CurrentScrapedResults)
            .Table($"{nameof(Watchdog)}{nameof(Watchdog.CurrentScrapedResults)}".TrimEnd('s'))
            .Element("Value", x =>
            {
                x.Not.Nullable();
                x.Length(10000);
            });
        
        mapping.HasMany(x => x.ScrapedResultsToNotifyAbout)
            .Table($"{nameof(Watchdog)}ScrapedResultToNotifyAbout")
            .Element("Value", x =>
            {
                x.Not.Nullable();
                x.Length(10000);
            });
    }
}