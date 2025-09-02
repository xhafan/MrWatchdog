using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Account.CompleteLogin;

namespace MrWatchdog.Web.Tests.Features.Account.CompleteLogin;

[TestFixture]
public class when_completing_login_for_existing_user_without_confirmed_login_token : BaseDatabaseTest
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
        
        ex.Message.ShouldContain("Login token has not been confirmed.");
    }

    private void _BuildEntities()
    {
        _loginToken = new LoginTokenBuilder(UnitOfWork).Build();
    }
}