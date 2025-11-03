using CoreDdd.Queries;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MrWatchdog.Core.Features.Account.Queries;
using MrWatchdog.Core.Infrastructure.ActingUserAccessors;
using MrWatchdog.Core.Infrastructure.Extensions;

namespace MrWatchdog.Web.Features.Shared.TagHelpers.Onboarding;

[HtmlTargetElement("onboarding")]
public class Onboarding(
    IHtmlHelper htmlHelper, 
    IQueryExecutor queryExecutor,
    IActingUserAccessor actingUserAccessor
)
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
        var userCompletedOnboardings = await GetUserCompleteOnboardings();

        return new OnboardingStimulusModel(
            EnableOnboarding: !userCompletedOnboardings.Contains(Identifier),
            OnboardingIdentifier: Identifier,
            Steps.WhereNotNull(),
            IsUserAuthenticated: actingUserAccessor.GetActingUserId() != 0
        );
    }

    private async Task<IEnumerable<string>> GetUserCompleteOnboardings()
    {
        var actingUserId = actingUserAccessor.GetActingUserId();
        if (actingUserId == 0) return [];

        var completeOnboardings = await queryExecutor.ExecuteAsync<GetUserCompleteOnboardingsQuery, string>(
            new GetUserCompleteOnboardingsQuery(actingUserId)
        );

        return completeOnboardings;
    }
}