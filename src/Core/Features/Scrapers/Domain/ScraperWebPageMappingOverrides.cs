using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class WatchdogWebPageMappingOverrides : IAutoMappingOverride<WatchdogWebPage>
{
    public void Override(AutoMapping<WatchdogWebPage> mapping)
    {
        mapping.HasMany(x => x.ScrapingResults)
            .Table($"{nameof(WatchdogWebPage)}{nameof(WatchdogWebPage.ScrapingResults)}".TrimEnd('s'))
            .Element("Value", x =>
            {
                x.Not.Nullable();
                x.Length(10000);
            });

        mapping.HasMany(x => x.HttpHeaders)
            .Table($"{nameof(WatchdogWebPage)}{nameof(WatchdogWebPage.HttpHeaders)}")
            .Component(c =>
            {
                c.Map(x => x.Name).Column("Name")
                    .Not.Nullable()
                    .Length(10000);
                c.Map(x => x.Value).Column("Value")
                    .Not.Nullable()
                    .Length(10000);
            });
    }
}