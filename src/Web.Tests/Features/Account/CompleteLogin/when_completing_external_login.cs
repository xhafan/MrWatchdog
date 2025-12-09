using System.Security.Claims;
using FakeItEasy;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Account.CompleteLogin;

namespace MrWatchdog.Web.Tests.Features.Account.CompleteLogin;

[TestFixture]
public class when_completing_external_login : BaseDatabaseTest
{
    private CompleteLoginController _controller = null!;
    private ICoreBus _bus = null!;
    private User _user = null!;
    private IAuthenticationService _authenticationService = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _bus = A.Fake<ICoreBus>();
        
        var urlHelper = A.Fake<IUrlHelper>();
        A.CallTo(() => urlHelper.IsLocalUrl("/Scrapers/Searches")).Returns(true);

        _controller = new CompleteLoginControllerBuilder(UnitOfWork)
            .WithBus(_bus)
            .WithUrlHelper(urlHelper)
            .Build();

        var serviceProvider = A.Fake<IServiceProvider>();
        _authenticationService = A.Fake<IAuthenticationService>();
        A.CallTo(() => serviceProvider.GetService(typeof(IAuthenticationService))).Returns(_authenticationService);
        
        _controller.ControllerContext = new ControllerContext(
            new ActionContext(
                new DefaultHttpContext
                {
                    RequestServices = serviceProvider,
                    User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Email, _user.Email)] , authenticationType: "Google"))
                }, 
                new RouteData(),
                new ControllerActionDescriptor()
            )
        );
        
        _actionResult = await _controller.CompleteExternalLoginCallback(returnUrl: "/Scrapers/Searches");
    }
    
    [Test]
    public void user_is_not_created()
    {
        A.CallTo(() => _bus.Send(A<CreateUserCommand>._)).MustNotHaveHappened();
    }

    [Test]
    public void user_is_logged_in()
    {
        A.CallTo(() => _authenticationService.SignInAsync(
                    A<HttpContext>._,
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    A<ClaimsPrincipal>.That.Matches(p => _MatchingClaimPrincipal(p)),
                    A<AuthenticationProperties?>.That.Matches(p => p!.IsPersistent == true)
                )
            )
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<RedirectResult>();
        var redirectResult = (RedirectResult) _actionResult;
        redirectResult.Url.ShouldBe("/Scrapers/Searches");
    }

    private bool _MatchingClaimPrincipal(ClaimsPrincipal claimsPrincipal)
    {
        claimsPrincipal.Identity.ShouldNotBeNull();
        claimsPrincipal.Identity.AuthenticationType.ShouldBe(CookieAuthenticationDefaults.AuthenticationScheme);

        claimsPrincipal.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value.ShouldBe(_user.Id.ToString());
        claimsPrincipal.Claims.Single(x => x.Type == ClaimTypes.Name).Value.ShouldBe(_user.Email);
        return true;
    }

    private void _BuildEntities()
    {
        _user = new UserBuilder(UnitOfWork).Build();
    }
}