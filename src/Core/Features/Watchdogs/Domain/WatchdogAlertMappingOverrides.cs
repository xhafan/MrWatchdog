using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class WatchdogAlertMappingOverrides : IAutoMappingOverride<WatchdogAlert>
{
    public void Override(AutoMapping<WatchdogAlert> mapping)
    {
        mapping.HasMany(x => x.CurrentScrapingResults)
            .Table($"{nameof(WatchdogAlert)}{nameof(WatchdogAlert.CurrentScrapingResults)}".TrimEnd('s'))
            .Element("Value", x =>
            {
                x.Not.Nullable();
                x.Length(10000);
            });
        
        mapping.HasMany(x => x.ScrapingResultsToAlertAbout)
            .Table($"{nameof(WatchdogAlert)}ScrapingResultToAlertAbout")
            .Element("Value", x =>
            {
                x.Not.Nullable();
                x.Length(10000);
            });
        
        mapping.HasMany(x => x.ScrapingResultsAlertHistory)
            .Table($"{nameof(WatchdogAlert)}ScrapingResultAlertHistory")
            .Component(x =>
            {
                x.Map(c => c.Result)
                    .Length(10000)
                    .Not.Nullable();
                x.Map(c => c.AlertedOn)
                    .Not.Nullable();
            });     
    }
}