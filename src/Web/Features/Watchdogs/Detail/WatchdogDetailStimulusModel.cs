using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Watchdogs.Detail;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public record WatchdogDetailStimulusModel(
    // ReSharper disable NotAccessedPositionalProperty.Global
    string DeleteWatchdogConfirmationMessageResource
    // ReSharper restore NotAccessedPositionalProperty.Global
);
