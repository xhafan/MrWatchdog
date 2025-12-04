using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Watchdogs.Search;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public record SearchStimulusModel(
    // ReSharper disable NotAccessedPositionalProperty.Global
    string DeleteWatchdogConfirmationMessageResource
    // ReSharper restore NotAccessedPositionalProperty.Global
);
