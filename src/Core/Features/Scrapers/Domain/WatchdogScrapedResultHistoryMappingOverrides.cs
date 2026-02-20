using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace MrWatchdog.Core.Features.Scrapers.Domain;

public class WatchdogScrapedResultHistoryMappingOverrides : IAutoMappingOverride<WatchdogScrapedResultHistory>
{
    public void Override(AutoMapping<WatchdogScrapedResultHistory> mapping)
    {
        var scrapedResultValueColumnName = $"{nameof(ScrapedResult)}{nameof(ScrapedResult.Value)}";
        var scrapedResultHashColumnName = $"{nameof(ScrapedResult)}{nameof(ScrapedResult.Hash)}";

        mapping.Component(x => x.ScrapedResult, c =>
        {
            c.Map(x => x.Value, scrapedResultValueColumnName)
                .Not.Nullable()
                .Length(10000);
            c.Map(x => x.Hash, scrapedResultHashColumnName)
                .Not.Nullable();
        });
    }
}