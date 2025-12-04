using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Shared.TagHelpers.Onboarding;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public record OnboardingStimulusModel(
    // ReSharper disable NotAccessedPositionalProperty.Global
    bool EnableOnboarding,
    string OnboardingIdentifier,
    IEnumerable<OnboardingStepStimulusModel> Steps,
    bool IsUserAuthenticated
    // ReSharper restore NotAccessedPositionalProperty.Global
);
