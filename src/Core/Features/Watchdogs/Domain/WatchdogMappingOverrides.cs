using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class WatchdogMappingOverrides : IAutoMappingOverride<Watchdog>
{
    public void Override(AutoMapping<Watchdog> mapping)
    {
        mapping.HasMany(x => x.CurrentScrapingResults)
            .Table($"{nameof(Watchdog)}{nameof(Watchdog.CurrentScrapingResults)}".TrimEnd('s'))
            .Element("Value", x =>
            {
                x.Not.Nullable();
                x.Length(10000);
            });
        
        mapping.HasMany(x => x.ScrapingResultsToNotifyAbout)
            .Table($"{nameof(Watchdog)}ScrapingResultToNotifyAbout")
            .Element("Value", x =>
            {
                x.Not.Nullable();
                x.Length(10000);
            });
        
        mapping.HasMany(x => x.ScrapingResultsHistory)
            .Table($"{nameof(Watchdog)}ScrapingResultHistory")
            .Component(x =>
            {
                x.Map(c => c.Result)
                    .Length(10000)
                    .Not.Nullable();
                x.Map(c => c.NotifiedOn)
                    .Not.Nullable();
            });     
    }
}