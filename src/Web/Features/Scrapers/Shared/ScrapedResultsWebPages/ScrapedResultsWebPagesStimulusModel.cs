using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Scrapers.Shared.ScrapedResultsWebPages;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public record ScrapedResultsWebPagesStimulusModel(
    // ReSharper disable NotAccessedPositionalProperty.Global
    string NoScrapedResultsAvailableResource,
    string NoScrapedResultsMatchingTheSearchTermAvailableResource
    // ReSharper restore NotAccessedPositionalProperty.Global
);
