using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class WatchdogWebPageMappingOverrides : IAutoMappingOverride<WatchdogWebPage>
{
    public void Override(AutoMapping<WatchdogWebPage> mapping)
    {
        mapping.HasMany(x => x.SelectedElements)
            .Table($"{nameof(WatchdogWebPage)}SelectedElement")
            .Element("Value", x =>
            {
                x.Not.Nullable();
                x.Length(10000);
            });
    }
}