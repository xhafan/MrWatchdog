using System.Security.Claims;
using FakeItEasy;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Account.CompleteLogin;

namespace MrWatchdog.Web.Tests.Features.Account.CompleteLogin;

[TestFixture]
public class when_completing_login_for_existing_superadmin_user : BaseDatabaseTest
{
    private CompleteLoginController _controller = null!;
    private ICoreBus _bus = null!;
    private LoginToken _loginToken = null!;
    private User _superAdminUser = null!;
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
    public void user_is_logged_in_with_superadmin_claim()
    {
        A.CallTo(() => _authenticationService.SignInAsync(
                    A<HttpContext>._,
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    A<ClaimsPrincipal>.That.Matches(p => _MatchingClaimPrincipal(p)),
                    A<AuthenticationProperties?>._
                )
            )
            .MustHaveHappenedOnceExactly();
    }

    private bool _MatchingClaimPrincipal(ClaimsPrincipal claimsPrincipal)
    {
        claimsPrincipal.Claims.Single(x => x.Type == CustomClaimTypes.SuperAdmin).Value.ShouldBe("true");
        return true;
    }

    private void _BuildEntities()
    {
        _superAdminUser = new UserBuilder(UnitOfWork)
            .WithSuperAdmin(true)
            .Build();

        _loginToken = new LoginTokenBuilder(UnitOfWork)
            .WithEmail(_superAdminUser.Email)
            .Build();
        _loginToken.Confirm();
    } 
}