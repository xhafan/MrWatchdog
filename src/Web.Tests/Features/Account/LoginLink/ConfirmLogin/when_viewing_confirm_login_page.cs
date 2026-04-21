using CoreBackend.Account.Features.LoginLink.Commands;
using CoreBackend.Account.Features.LoginLink.Domain;
using CoreBackend.Infrastructure.Rebus;
using CoreBackend.TestsShared;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.TestsShared.Builders;
using MrWatchdog.Web.Features.Account.ConfirmLogin;

namespace MrWatchdog.Web.Tests.Features.Account.LoginLink.ConfirmLogin;

[TestFixture]
public class when_viewing_confirm_login_page : BaseDatabaseTest
{
    private ConfirmLoginModel _model = null!;
    private IActionResult _actionResult = null!;
    private ICoreBus _bus = null!;
    private LoginToken _loginToken = null!;
    
    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _bus = A.Fake<ICoreBus>();
        
        _model = new ConfirmLoginModelBuilder(UnitOfWork)
            .WithBus(_bus)
            .WithLoginToken(Uri.EscapeDataString(_loginToken.Token))
            .Build();
        
        _actionResult = await _model.OnGet();
    }
    
    [Test]
    public void command_is_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(A<ConfirmLoginTokenCommand>.That.Matches(p => p.LoginTokenGuid == _loginToken.Guid))).MustHaveHappenedOnceExactly();
    } 
    
    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<PageResult>();
    }    
    
    [Test]
    public void model_is_valid()
    {
        _model.ModelState.IsValid.ShouldBe(true);
        _model.ReturnUrl.ShouldBe(LoginTokenBuilder.TokenReturnUrl);
    }
    
    private void _BuildEntities()
    {
        _loginToken = new LoginTokenBuilder(UnitOfWork).Build();
    }
}