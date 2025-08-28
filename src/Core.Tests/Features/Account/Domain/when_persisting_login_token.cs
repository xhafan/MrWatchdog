using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Account.Domain;

[TestFixture]
public class when_persisting_login_token : BaseDatabaseTest
{
    private LoginToken _newLoginToken = null!;
    private LoginToken? _persistedLoginToken;

    [SetUp]
    public void Context()
    {
        _newLoginToken = new LoginTokenBuilder(UnitOfWork).Build();
        
        UnitOfWork.Flush();
        UnitOfWork.Clear();

        _persistedLoginToken = UnitOfWork.Get<LoginToken>(_newLoginToken.Id);
    }

    [Test]
    public void persisted_login_token_can_be_retrieved_and_has_correct_data()
    {
        _persistedLoginToken.ShouldNotBeNull();
        _persistedLoginToken.ShouldBe(_newLoginToken);

        _persistedLoginToken.Guid.ShouldBe(_newLoginToken.Guid);
        _persistedLoginToken.Email.ShouldBe(_newLoginToken.Email);
        _persistedLoginToken.Token.ShouldBe(_newLoginToken.Token);
        _persistedLoginToken.Confirmed.ShouldBe(false);
        _persistedLoginToken.Used.ShouldBe(false);
    }
}