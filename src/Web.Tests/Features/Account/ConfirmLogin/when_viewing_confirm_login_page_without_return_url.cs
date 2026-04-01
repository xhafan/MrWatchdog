using CoreBackend.Infrastructure.Rebus;
using CoreBackend.TestsShared;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.TestsShared.Builders;
using MrWatchdog.Web.Features.Account.ConfirmLogin;

namespace MrWatchdog.Web.Tests.Features.Account.ConfirmLogin;

[TestFixture]
public class when_viewing_confirm_login_page_without_return_url : BaseDatabaseTest
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
            .WithToken(Uri.EscapeDataString(_loginToken.Token))
            .Build();
        
        _actionResult = await _model.OnGet();
    }
    
    [Test]
    public void return_url_is_correct()
    {
        _model.ReturnUrl.ShouldBe("/");
    }
    
    private void _BuildEntities()
    {
        _loginToken = new LoginTokenBuilder(UnitOfWork)
            .WithTokenReturnUrl(null)
            .Build();
    }
}