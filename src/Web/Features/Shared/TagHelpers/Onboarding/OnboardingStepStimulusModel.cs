using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Shared.TagHelpers.Onboarding;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public record OnboardingStepStimulusModel(
    // ReSharper disable NotAccessedPositionalProperty.Global
    string Text,
    string? ElementIdentifier = null,
    string? Title = null
    // ReSharper restore NotAccessedPositionalProperty.Global
);
