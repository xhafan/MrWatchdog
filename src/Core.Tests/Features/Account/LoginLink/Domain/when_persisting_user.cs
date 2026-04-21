using CoreBackend.TestsShared;
using CoreDdd.Nhibernate.TestHelpers;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Account.LoginLink.Domain;

[TestFixture]
public class when_persisting_user : BaseDatabaseTest
{
    private User _newUser = null!;
    private User? _persistedUser;

    [SetUp]
    public void Context()
    {
        _newUser = new UserBuilder(UnitOfWork).Build();
        _newUser.CompleteOnboarding(OnboardingIdentifiers.ScraperScrapedResults);
        
        UnitOfWork.Flush();
        UnitOfWork.Clear();

        _persistedUser = UnitOfWork.Get<User>(_newUser.Id);
    }

    [Test]
    public void persisted_user_can_be_retrieved_and_has_correct_data()
    {
        _persistedUser.ShouldNotBeNull();
        _persistedUser.ShouldBe(_newUser);

        _persistedUser.Email.ShouldBe(_newUser.Email);
        _persistedUser.SuperAdmin.ShouldBe(false);
        _persistedUser.Culture.ShouldBe(UserBuilder.Culture);

        _persistedUser.CompleteOnboardings.ShouldBe([OnboardingIdentifiers.ScraperScrapedResults]);
    }
}