using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Account;

namespace MrWatchdog.Web.Tests.Features.Account;

[TestFixture]
public class when_getting_login_token_confirmation : BaseDatabaseTest
{
    private LoginController _controller = null!;
    private LoginToken _loginToken = null!;
    
    [TestCase(true, TestName = "1 login token confirmed")]
    [TestCase(false, TestName = "2 login token not confirmed")]
    public async Task confirmation_is_correct(bool loginTokenIsConfirmed)
    {
        _BuildEntities(loginTokenIsConfirmed);
        
        _controller = new LoginControllerBuilder(UnitOfWork).Build();

        var confirmed = await _controller.GetLoginTokenConfirmation(_loginToken.Guid);
        
        confirmed.ShouldBe(loginTokenIsConfirmed);
    } 

    private void _BuildEntities(bool loginTokenIsConfirmed)
    {
        _loginToken = new LoginTokenBuilder(UnitOfWork).Build();
        if (loginTokenIsConfirmed)
        {
            _loginToken.Confirm();
        }
    } 
}