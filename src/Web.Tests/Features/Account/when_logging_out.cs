using FakeItEasy;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Account;

namespace MrWatchdog.Web.Tests.Features.Account;

[TestFixture]
public class when_logging_out : BaseDatabaseTest
{
    private LoginController _controller = null!;
    private IAuthenticationService _authenticationService = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _controller = new LoginControllerBuilder(UnitOfWork).Build();

        var serviceProvider = A.Fake<IServiceProvider>();
        _authenticationService = A.Fake<IAuthenticationService>();
        A.CallTo(() => serviceProvider.GetService(typeof(IAuthenticationService))).Returns(_authenticationService);
        
        _controller.ControllerContext = new ControllerContext(
            new ActionContext(
                new DefaultHttpContext
                {
                    RequestServices = serviceProvider
                }, 
                new RouteData(),
                new ControllerActionDescriptor()
            )
        );
        _actionResult = await _controller.Logout();
    }
    
    [Test]
    public void user_is_logged_out()
    {
        A.CallTo(() => _authenticationService.SignOutAsync(A<HttpContext>._, A<string?>._, A<AuthenticationProperties?>._)).MustHaveHappenedOnceExactly();
    }
    
    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<RedirectResult>();
        var redirectResult = (RedirectResult) _actionResult;
        redirectResult.Url.ShouldBe("/");
    }    
}