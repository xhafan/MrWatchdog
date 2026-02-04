using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Scrapers.Detail.WebPage;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public record WebPageScrapedResultsStimulusModel(
    // ReSharper disable NotAccessedPositionalProperty.Global
    string ScrapedOnInIso8601Format
    // ReSharper restore NotAccessedPositionalProperty.Global
);
