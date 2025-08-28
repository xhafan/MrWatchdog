using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Account.Domain;

[TestFixture]
public class when_marking_unconfirmed_login_token_as_used
{
    private LoginToken _loginToken = null!;

    [SetUp]
    public void Context()
    {
        _loginToken = new LoginTokenBuilder().Build();
    }

    [Test]
    public void exception_is_thrown()
    {
        var ex = Should.Throw<Exception>(() => _loginToken.MarkAsUsed());
        
        ex.Message.ShouldBe("Login token has not been confirmed.");
    }
}