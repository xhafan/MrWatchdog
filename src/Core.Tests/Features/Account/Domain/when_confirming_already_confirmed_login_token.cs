using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Account.Domain;

[TestFixture]
public class when_confirming_already_confirmed_login_token
{
    private LoginToken _loginToken = null!;

    [SetUp]
    public void Context()
    {
        _loginToken = new LoginTokenBuilder().Build();
        _loginToken.Confirm();
    }

    [Test]
    public void exception_is_thrown()
    {
        var ex = Should.Throw<Exception>(() => _loginToken.Confirm());
        
        ex.Message.ShouldBe("Login token has already been confirmed.");
    }
}