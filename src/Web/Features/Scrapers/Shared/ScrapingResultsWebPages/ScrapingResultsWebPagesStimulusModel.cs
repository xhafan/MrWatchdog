using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Scrapers.Shared.ScrapingResultsWebPages;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public record ScrapingResultsWebPagesStimulusModel(
    // ReSharper disable NotAccessedPositionalProperty.Global
    string NoScrapedResultsAvailableResource,
    string NoScrapedResultsMatchingTheSearchTermAvailableResource
    // ReSharper restore NotAccessedPositionalProperty.Global
);
