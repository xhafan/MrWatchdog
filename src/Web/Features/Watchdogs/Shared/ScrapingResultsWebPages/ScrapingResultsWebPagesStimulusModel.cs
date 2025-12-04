using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Watchdogs.Shared.ScrapingResultsWebPages;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public record ScrapingResultsWebPagesStimulusModel(
    // ReSharper disable NotAccessedPositionalProperty.Global
    string NoScrapedResultsAvailableResource,
    string NoScrapedResultsMatchingTheSearchTermAvailableResource
    // ReSharper restore NotAccessedPositionalProperty.Global
);
