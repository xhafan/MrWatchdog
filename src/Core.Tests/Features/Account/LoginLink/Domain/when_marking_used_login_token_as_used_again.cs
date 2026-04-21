using CoreBackend.Account.Features.LoginLink.Domain;
using MrWatchdog.Core.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Account.LoginLink.Domain;

[TestFixture]
public class when_marking_used_login_token_as_used_again
{
    private LoginToken _loginToken = null!;

    [SetUp]
    public void Context()
    {
        _loginToken = new LoginTokenBuilder().Build();
        _loginToken.Confirm();
        _loginToken.MarkAsUsed();
    }

    [Test]
    public void exception_is_thrown()
    {
        var ex = Should.Throw<Exception>(() => _loginToken.MarkAsUsed());
        
        ex.Message.ShouldBe("Login token has been already used.");
    }
}