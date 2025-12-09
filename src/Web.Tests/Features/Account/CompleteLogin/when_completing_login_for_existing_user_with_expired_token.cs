using FakeItEasy;
using Microsoft.AspNetCore.Authentication;
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
public class when_completing_login_for_existing_user_with_expired_token : BaseDatabaseTest
{
    private CompleteLoginController _controller = null!;
    private ICoreBus _bus = null!;
    private LoginToken _loginToken = null!;
    private User _user = null!;

    [SetUp]
    public void Context()
    {
        _BuildEntities();
        
        _bus = A.Fake<ICoreBus>();
        
        _controller = new CompleteLoginControllerBuilder(UnitOfWork)
            .WithBus(_bus)
            .Build();

        var serviceProvider = A.Fake<IServiceProvider>();
        A.CallTo(() => serviceProvider.GetService(typeof(IAuthenticationService))).Returns(A.Fake<IAuthenticationService>());
        
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
    }
    
    [Test]
    public void exception_is_thrown()
    {
        var ex = Should.Throw<Exception>(async () => await _controller.CompleteLogin(_loginToken.Guid));
        
        ex.Message.ShouldContain("The token is expired");
    }

    private void _BuildEntities()
    {
        _user = new UserBuilder(UnitOfWork).Build();

        var jwtOptions = OptionsTestRetriever.Retrieve<JwtOptions>().Value;

        var loginTokenGuid = Guid.NewGuid();
        _loginToken = new LoginTokenBuilder(UnitOfWork)
            .WithGuid(loginTokenGuid)
            .WithToken(TokenGenerator.GenerateToken(
                loginTokenGuid, 
                _user.Email, 
                returnUrl: "/Scrapers/Searches", 
                jwtOptions,
                validFrom: DateTime.UtcNow.AddMinutes(-jwtOptions.ExpireMinutes - 1)
            ))
            .Build();
        _loginToken.Confirm();
    } 
}