using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Account.ConfirmLogin;

namespace MrWatchdog.Web.Tests.Features.Account.ConfirmLogin;

[TestFixture]
public class when_viewing_confirm_login_page_without_login_token : BaseDatabaseTest
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
            .WithToken(null)
            .Build();
        
        _actionResult = await _model.OnGet();
    }
    
    [Test]
    public void command_is_not_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(A<ConfirmLoginTokenCommand>.That.Matches(p => p.LoginTokenGuid == _loginToken.Guid))).MustNotHaveHappened();
    } 
    
    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<BadRequestObjectResult>();
    }    
    
    [Test]
    public void model_is_valid()
    {
        _model.ModelState.IsValid.ShouldBe(false);
    }
    
    private void _BuildEntities()
    {
        _loginToken = new LoginTokenBuilder(UnitOfWork).Build();
    }
}