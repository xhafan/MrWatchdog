using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Watchdogs.Detail.Actions;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public record ActionsStimulusModel(
    // ReSharper disable NotAccessedPositionalProperty.Global
    string RequestToMakeWebScraperPublicConfirmationMessageResource,
    string MakeWebScraperPublicConfirmationMessageResource,
    string MakeWebScraperPrivateConfirmationMessageResource
    // ReSharper restore NotAccessedPositionalProperty.Global
);
