using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Account.Queries;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Account.Queries;

[TestFixture]
public class when_querying_user_complete_onboardings : BaseDatabaseTest
{
    private IEnumerable<string> _completeOnboardings = null!;

    [SetUp]
    public async Task Context()
    {
        var user = new UserBuilder(UnitOfWork).Build();
        user.CompleteOnboarding(OnboardingIdentifiers.ScraperScrapingResults);

        var queryHandler = new GetUserCompleteOnboardingsQueryHandler(UnitOfWork);

        _completeOnboardings = await queryHandler.ExecuteAsync<string>(new GetUserCompleteOnboardingsQuery(user.Id));
    }

    [Test]
    public void complete_onboardings_are_correct()
    {
        _completeOnboardings.ShouldBe([OnboardingIdentifiers.ScraperScrapingResults]);
    }
}