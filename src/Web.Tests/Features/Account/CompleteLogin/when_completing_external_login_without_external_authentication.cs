using FakeItEasy;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Account.CompleteLogin;
using System.Security.Claims;

namespace MrWatchdog.Web.Tests.Features.Account.CompleteLogin;

[TestFixture]
public class when_completing_external_login_without_external_authentication : BaseDatabaseTest
{
    private CompleteLoginController _controller = null!;
    private ICoreBus _bus = null!;
    private User _user = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _bus = A.Fake<ICoreBus>();
        
        _controller = new CompleteLoginControllerBuilder(UnitOfWork)
            .WithBus(_bus)
            .Build();

        _controller.ControllerContext = new ControllerContext(
            new ActionContext(
                new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            [new Claim(ClaimTypes.Email, _user.Email)],
                            authenticationType: CookieAuthenticationDefaults.AuthenticationScheme)
                    )
                },
                new RouteData(),
                new ControllerActionDescriptor()
            )
        );
        
        _actionResult = await _controller.CompleteExternalLoginCallback();
    }
    
    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<UnauthorizedResult>();
    }

    private void _BuildEntities()
    {
        _user = new UserBuilder(UnitOfWork).Build();
    }
}