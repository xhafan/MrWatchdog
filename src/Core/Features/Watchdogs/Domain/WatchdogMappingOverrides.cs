using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace MrWatchdog.Core.Features.Scrapers.Domain;

public class WatchdogSearchMappingOverrides : IAutoMappingOverride<WatchdogSearch>
{
    public void Override(AutoMapping<WatchdogSearch> mapping)
    {
        mapping.HasMany(x => x.CurrentScrapingResults)
            .Table($"{nameof(WatchdogSearch)}{nameof(WatchdogSearch.CurrentScrapingResults)}".TrimEnd('s'))
            .Element("Value", x =>
            {
                x.Not.Nullable();
                x.Length(10000);
            });
        
        mapping.HasMany(x => x.ScrapingResultsToNotifyAbout)
            .Table($"{nameof(WatchdogSearch)}ScrapingResultToNotifyAbout")
            .Element("Value", x =>
            {
                x.Not.Nullable();
                x.Length(10000);
            });
        
        mapping.HasMany(x => x.ScrapingResultsHistory)
            .Table($"{nameof(WatchdogSearch)}ScrapingResultHistory")
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