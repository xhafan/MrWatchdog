using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MrWatchdog.Core.Infrastructure.Extensions;

namespace MrWatchdog.Web.Features.Shared.TagHelpers.Onboarding;

[HtmlTargetElement("onboarding")]
public class Onboarding(IHtmlHelper htmlHelper)
    : BaseStimulusModelViewTagHelper<OnboardingStimulusModel>(htmlHelper)
{
    public string Identifier { get; set; } = null!;
    public IEnumerable<OnboardingStepStimulusModel> Steps { get; set; } = null!;

    protected override string GetStimulusControllerName()
    {
        return StimulusControllers.Onboarding;
    }

    protected override async Task<OnboardingStimulusModel> GetStimulusModel()
    {
        var userCompletedOnboardings = await GetUserCompletedOnboardings();

        return new OnboardingStimulusModel(
            EnableOnboarding: !userCompletedOnboardings.Contains(Identifier),
            OnboardingIdentifier: Identifier,
            Steps.WhereNotNull()
        );
    }

    private Task<IEnumerable<string>> GetUserCompletedOnboardings()
    {
        return Task.FromResult<IEnumerable<string>>([
            //OnboardingIdentifiers.WatchdogsScrapingResults
        ]);
    }
}