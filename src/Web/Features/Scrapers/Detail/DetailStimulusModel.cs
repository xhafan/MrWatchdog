using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Watchdogs.Detail;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public record DetailStimulusModel(
    // ReSharper disable NotAccessedPositionalProperty.Global
    long WatchdogId,
    string DeleteWebScraperConfirmationMessageResource
    // ReSharper restore NotAccessedPositionalProperty.Global
);
