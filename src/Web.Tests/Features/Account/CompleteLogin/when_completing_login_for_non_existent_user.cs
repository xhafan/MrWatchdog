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
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;
using MrWatchdog.Web.Features.Account.CompleteLogin;
using System.Security.Claims;

namespace MrWatchdog.Web.Tests.Features.Account.CompleteLogin;

[TestFixture]
public class when_completing_login_for_non_existent_user : BaseDatabaseTest
{
    private readonly string _tokenEmail = $"user+{Guid.NewGuid()}@email.com";
    
    private IActionResult _actionResult = null!;
    private CompleteLoginController _controller = null!;
    private ICoreBus _bus = null!;
    private LoginToken _loginToken = null!;
    private User? _user;
    private IAuthenticationService _authenticationService = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntitiesInSeparateTransaction();
        
        _bus = A.Fake<ICoreBus>();
        
        _SimulateUserCreationOnCreateUserCommand();
        _SimulateMarkingLoginTokenAsUsedOnMarkLoginTokenAsUsedCommand();
        
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
        
        _actionResult = await _controller.CompleteLogin(_loginToken.Guid);
    }
    
    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<OkObjectResult>();
        var okObjectResult = (OkObjectResult) _actionResult;
        okObjectResult.Value.ShouldBe(LoginTokenBuilder.TokenReturnUrl);
    }

    [Test]
    public void model_is_valid()
    {
        _controller.ModelState.IsValid.ShouldBe(true);
    }
    
    [Test]
    public void user_is_created()
    {
        _user.ShouldNotBeNull();
    } 
    
    [Test]
    public void login_token_is_marked_as_used()
    {
        UnitOfWork.Clear();
        
        _loginToken = UnitOfWork.LoadById<LoginToken>(_loginToken.Id);
        _loginToken.Used.ShouldBe(true);
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

        claimsPrincipal.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value.ShouldBe(_user!.Id.ToString());
        claimsPrincipal.Claims.Single(x => x.Type == ClaimTypes.Name).Value.ShouldBe(_user!.Email);
        return true;
    }

    [TearDown]
    public async Task TearDown()
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();
        await newUnitOfWork.DeleteUserCascade(_user);
        await newUnitOfWork.DeleteLoginTokenCascade(_loginToken);
    }
    
    private void _SimulateUserCreationOnCreateUserCommand()
    {
        A.CallTo(() =>
                _bus.Send(
                    A<CreateUserCommand>.That.Matches(p =>
                        p.Email == _tokenEmail
                    )
                )
            )
            .Invokes(_ =>
            {
                // simulate the command handler in a separate transaction
                using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
                newUnitOfWork.BeginTransaction();

                _user = new UserBuilder(newUnitOfWork)
                    .WithEmail(_tokenEmail)
                    .Build();
            });
    }
    
    private void _SimulateMarkingLoginTokenAsUsedOnMarkLoginTokenAsUsedCommand()
    {
        A.CallTo(() =>
                _bus.Send(
                    A<MarkLoginTokenAsUsedCommand>.That.Matches(p =>
                        p.LoginTokenGuid == _loginToken.Guid
                    )
                )
            )
            .Invokes(_ =>
            {
                // simulate the command handler in a separate transaction
                using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
                newUnitOfWork.BeginTransaction();

                var loginToken = newUnitOfWork.LoadById<LoginToken>(_loginToken.Id);
                loginToken.MarkAsUsed();
            });
    }    
    
    private void _BuildEntitiesInSeparateTransaction()
    {        
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();
        
        _loginToken = new LoginTokenBuilder(newUnitOfWork)
            .WithEmail(_tokenEmail)
            .Build();
        _loginToken.Confirm();
    }    
}