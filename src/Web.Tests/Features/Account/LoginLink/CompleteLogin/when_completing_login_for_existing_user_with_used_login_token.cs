using CoreBackend.Account.Features.LoginLink.Domain;
using CoreBackend.TestsShared;
using MrWatchdog.Core.TestsShared.Builders;
using MrWatchdog.Web.Features.Account.LoginLink.CompleteLogin;

namespace MrWatchdog.Web.Tests.Features.Account.LoginLink.CompleteLogin;

[TestFixture]
public class when_completing_login_for_existing_user_with_used_login_token : BaseDatabaseTest
{
    private CompleteLoginController _controller = null!;
    private LoginToken _loginToken = null!;

    [SetUp]
    public void Context()
    {
        _BuildEntities();
        
        _controller = new CompleteLoginControllerBuilder(UnitOfWork).Build();
    }
    
    [Test]
    public void exception_is_thrown()
    {
        var ex = Should.Throw<Exception>(async () => await _controller.CompleteLogin(_loginToken.Guid));
        
        ex.Message.ShouldContain("Login token has been already used.");
    }
    
    private void _BuildEntities()
    {
        _loginToken = new LoginTokenBuilder(UnitOfWork).Build();
        _loginToken.Confirm();
        _loginToken.MarkAsUsed();
    }
}