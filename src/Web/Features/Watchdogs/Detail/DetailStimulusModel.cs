using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Watchdogs.Detail;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public record DetailStimulusModel(
    long WatchdogId,
    string DeleteWebScraperConfirmationMessageResource
);
