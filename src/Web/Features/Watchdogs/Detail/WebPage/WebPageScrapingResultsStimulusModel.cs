using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;

[TsInterface(IncludeNamespace = false, AutoI = false)]
// ReSharper disable once NotAccessedPositionalProperty.Global
public record WebPageScrapingResultsStimulusModel(string ScrapedOnInIso8601Format);
