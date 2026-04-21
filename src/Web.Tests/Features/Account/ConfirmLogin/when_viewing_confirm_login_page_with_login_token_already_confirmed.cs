using CoreBackend.Account.Features.LoginLink.Commands;
using CoreBackend.Account.Features.LoginLink.Domain;
using CoreBackend.Infrastructure.Rebus;
using CoreBackend.TestsShared;
using CoreDdd.Nhibernate.TestHelpers;
using FakeItEasy;
using MrWatchdog.Core.TestsShared.Builders;
using MrWatchdog.Web.Features.Account.ConfirmLogin;

namespace MrWatchdog.Web.Tests.Features.Account.ConfirmLogin;

[TestFixture]
public class when_viewing_confirm_login_page_with_login_token_already_confirmed : BaseDatabaseTest
{
    private ConfirmLoginModel _model = null!;
    private ICoreBus _bus = null!;
    private LoginToken _loginToken = null!;
    
    [SetUp]
    public void Context()
    {
        _BuildEntities();
        
        _bus = A.Fake<ICoreBus>();
        
        _model = new ConfirmLoginModelBuilder(UnitOfWork)
            .WithBus(_bus)
            .WithLoginToken(Uri.EscapeDataString(_loginToken.Token))
            .Build();
    }
    
    [Test]
    public void exception_is_thrown()
    {
        var ex = Should.Throw<Exception>(async () => await _model.OnGet());
        
        ex.Message.ShouldBe("Login token has already been confirmed.");
    } 
    
    [Test]
    public void command_is_not_sent_over_message_bus()
    {
        Should.Throw<Exception>(async () => await _model.OnGet());
        
        A.CallTo(() => _bus.Send(A<ConfirmLoginTokenCommand>._)).MustNotHaveHappened();
    }     
    
    private void _BuildEntities()
    {
        _loginToken = new LoginTokenBuilder(UnitOfWork).Build();
        _loginToken.Confirm();
        UnitOfWork.Save(_loginToken);
    }    
}