using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace MrWatchdog.Core.Features.Scrapers.Domain;

public class ScraperWebPageMappingOverrides : IAutoMappingOverride<ScraperWebPage>
{
    public void Override(AutoMapping<ScraperWebPage> mapping)
    {
        mapping.HasMany(x => x.ScrapedResults)
            .Table($"{nameof(ScraperWebPage)}{nameof(ScraperWebPage.ScrapedResults)}".TrimEnd('s'))
            .Element("Value", x =>
            {
                x.Not.Nullable();
                x.Length(10000);
            });

        mapping.HasMany(x => x.HttpHeaders)
            .Table($"{nameof(ScraperWebPage)}{nameof(ScraperWebPage.HttpHeaders)}".TrimEnd('s'))
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