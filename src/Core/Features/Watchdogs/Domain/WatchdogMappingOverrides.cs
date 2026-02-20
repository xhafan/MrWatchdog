using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using MrWatchdog.Core.Features.Scrapers.Domain;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class WatchdogMappingOverrides : IAutoMappingOverride<Watchdog>
{
    public void Override(AutoMapping<Watchdog> mapping)
    {
        var scrapedResultValueColumnName = $"{nameof(ScrapedResult)}{nameof(ScrapedResult.Value)}";
        var scrapedResultHashColumnName = $"{nameof(ScrapedResult)}{nameof(ScrapedResult.Hash)}";

        mapping.HasMany(x => x.CurrentScrapedResults)
            .Table($"{nameof(Watchdog)}{nameof(Watchdog.CurrentScrapedResults)}".TrimEnd('s'))
            .Component(c =>
            {
                c.Map(x => x.Value, scrapedResultValueColumnName)
                    .Not.Nullable()
                    .Length(10000);
                c.Map(x => x.Hash, scrapedResultHashColumnName)
                    .Not.Nullable();
            });
        
        mapping.HasMany(x => x.ScrapedResultsToNotifyAbout)
            .Table($"{nameof(Watchdog)}ScrapedResultToNotifyAbout")
            .Component(c =>
            {
                c.Map(x => x.Value, scrapedResultValueColumnName)
                    .Not.Nullable()
                    .Length(10000);
                c.Map(x => x.Hash, scrapedResultHashColumnName)
                    .Not.Nullable();
            });
    }
}