using Reinforced.Typings.Attributes;

namespace MrWatchdog.Core.Features.Account;

[TsClass(IncludeNamespace = false, AutoExportMethods = false)]
public static class UserUrlConstants
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string OnboardingIdentifierVariable = "$onboardingIdentifier";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ApiCompleteOnboardingUrlTemplate = $"/api/Users/CompleteOnboarding?onboardingIdentifier={OnboardingIdentifierVariable}";

    public static string WithOnboardingIdentifier(this string urlTemplate, string onboardingIdentifier)
    {
        return urlTemplate.WithVariable(OnboardingIdentifierVariable, onboardingIdentifier);
    }
}