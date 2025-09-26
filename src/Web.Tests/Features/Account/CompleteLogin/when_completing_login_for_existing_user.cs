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
using System.Security.Claims;

namespace MrWatchdog.Web.Tests.Features.Account.CompleteLogin;

[TestFixture]
public class when_completing_login_for_existing_user : BaseDatabaseTest
{
    private CompleteLoginController _controller = null!;
    private ICoreBus _bus = null!;
    private LoginToken _loginToken = null!;
    private User _user = null!;
    private IAuthenticationService _authenticationService = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _bus = A.Fake<ICoreBus>();
        
        _controller = new CompleteLoginControllerBuilder(UnitOfWork)
            .WithBus(_bus)
            .Build();

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
        
        await _controller.CompleteLogin(_loginToken.Guid);
    }
    
    [Test]
    public void user_is_not_created()
    {
       A.CallTo(() => _bus.Send(A<CreateUserCommand>.That.Matches(p => p.Email == "user@email.com"))).MustNotHaveHappened();
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

        _loginToken = new LoginTokenBuilder(UnitOfWork)
            .WithEmail(_user.Email)
            .Build();
        _loginToken.Confirm();
    } 
}