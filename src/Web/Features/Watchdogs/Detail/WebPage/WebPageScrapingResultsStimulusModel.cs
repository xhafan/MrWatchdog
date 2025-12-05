using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public record WebPageScrapingResultsStimulusModel(
    // ReSharper disable NotAccessedPositionalProperty.Global
    string ScrapedOnInIso8601Format
    // ReSharper restore NotAccessedPositionalProperty.Global
);
