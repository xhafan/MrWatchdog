using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Scrapers.Detail;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public record DetailStimulusModel(
    // ReSharper disable NotAccessedPositionalProperty.Global
    long ScraperId,
    string DeleteScraperConfirmationMessageResource
    // ReSharper restore NotAccessedPositionalProperty.Global
);
