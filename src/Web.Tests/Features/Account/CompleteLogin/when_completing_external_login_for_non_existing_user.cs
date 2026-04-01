using CoreDdd.Nhibernate.UnitOfWorks;
using FakeItEasy;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Infrastructure.Localization;
using MrWatchdog.Web.Features.Account.CompleteLogin;
using System.Globalization;
using System.Security.Claims;
using CoreBackend.Infrastructure.Rebus;
using CoreBackend.TestsShared;
using MrWatchdog.Core.TestsShared;
using MrWatchdog.Core.TestsShared.Builders;

namespace MrWatchdog.Web.Tests.Features.Account.CompleteLogin;

[TestFixture]
public class when_completing_external_login_for_non_existing_user : BaseDatabaseTest
{
    private readonly string _email = $"user+{Guid.NewGuid()}@email.com";

    private CompleteLoginController _controller = null!;
    private ICoreBus _bus = null!;
    private IAuthenticationService _authenticationService = null!;
    private IActionResult _actionResult = null!;
    private User _user = null!;
    private CultureInfo _originalUiCulture = null!;

    [SetUp]
    public async Task Context()
    {
        _originalUiCulture = CultureInfo.CurrentUICulture;
        
        CultureInfo.CurrentUICulture = CultureConstants.Cs;

        _bus = A.Fake<ICoreBus>();
        
        var urlHelper = A.Fake<IUrlHelper>();
        A.CallTo(() => urlHelper.IsLocalUrl("/Watchdogs")).Returns(true);

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
                    User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Email, _email)] , authenticationType: "Google"))
                }, 
                new RouteData(),
                new ControllerActionDescriptor()
            )
        );

        _SimulateUserCreationOnCreateUserCommand();
        
        _actionResult = await _controller.CompleteExternalLoginCallback(returnUrl: "/Watchdogs");
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
        claimsPrincipal.Claims.Single(x => x.Type == ClaimTypes.Name).Value.ShouldBe(_email);
        return true;
    }

    [Test]
    public void user_is_created_correctly()
    {
        _user.ShouldNotBeNull();
        _user.Email.ShouldBe(_email);
        _user.Culture.ShouldBe(CultureConstants.Cs);
    }


    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<RedirectResult>();
        var redirectResult = (RedirectResult) _actionResult;
        redirectResult.Url.ShouldBe("/Watchdogs");
    }

    [TearDown]
    public async Task TearDown()
    {
        await NhibernateUnitOfWorkRunner.RunAsync(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            async newUnitOfWork =>
            {
                await newUnitOfWork.DeleteUserCascade(_user);
            }
        );

        CultureInfo.CurrentUICulture = _originalUiCulture;
    }

    private void _SimulateUserCreationOnCreateUserCommand()
    {
        A.CallTo(() => _bus.Send(A<CreateUserCommand>._))
            .Invokes(call =>
            {
                // simulate the command handler in a separate transaction
                NhibernateUnitOfWorkRunner.Run(
                    () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
                    newUnitOfWork =>
                    {
                        var command = (CreateUserCommand)call.Arguments.Single()!;

                        _user = new UserBuilder(newUnitOfWork)
                            .WithEmail(command.Email)
                            .WithCulture(command.Culture)
                            .Build();
                    }
                );
            });
    }

}