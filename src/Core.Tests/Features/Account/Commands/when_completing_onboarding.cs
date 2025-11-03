using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Account.Commands;

[TestFixture]
public class when_completing_onboarding : BaseDatabaseTest
{
    private User _user = null!;

    [SetUp]
    public async Task Context()
    {
        _user = new UserBuilder(UnitOfWork).Build();

        var handler = new CompleteOnboardingCommandMessageHandler(new UserRepository(UnitOfWork));

        await handler.Handle(new CompleteOnboardingCommand(OnboardingIdentifiers.WatchdogsScrapingResults) {ActingUserId = _user.Id});
    }

    [Test]
    public void user_onboarding_is_complete()
    {
        _user.CompleteOnboardings.ShouldBe([OnboardingIdentifiers.WatchdogsScrapingResults]);
    }
}