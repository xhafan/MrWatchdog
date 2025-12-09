using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public record WebPageOverviewStimulusModel(
    // ReSharper disable NotAccessedPositionalProperty.Global
    bool IsEmptyWebPage
    // ReSharper restore NotAccessedPositionalProperty.Global
);
