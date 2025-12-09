using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Scrapers.Detail.Actions;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public record ActionsStimulusModel(
    // ReSharper disable NotAccessedPositionalProperty.Global
    string RequestToMakeScraperPublicConfirmationMessageResource,
    string MakeScraperPublicConfirmationMessageResource,
    string MakeScraperPrivateConfirmationMessageResource
    // ReSharper restore NotAccessedPositionalProperty.Global
);
