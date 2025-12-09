using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Scrapers.Detail.WebPage;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public record WebPageStimulusModel(
    // ReSharper disable NotAccessedPositionalProperty.Global
    string RemoveWebPageConfirmationMessageResource
    // ReSharper restore NotAccessedPositionalProperty.Global
);
