using CoreDdd.Nhibernate.UnitOfWorks;
using FakeItEasy;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Account.CompleteLogin;

namespace MrWatchdog.Web.Tests.Features.Account.CompleteLogin;

[TestFixture]
public class when_completing_login_for_existing_user_without_return_url : BaseDatabaseTest
{
    private IActionResult _actionResult = null!;
    private CompleteLoginController _controller = null!;
    private ICoreBus _bus = null!;
    private LoginToken _loginToken = null!;
    private User _user = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        await UnitOfWork.CommitAsync();
        UnitOfWork.BeginTransaction();
        
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
        
        _actionResult = await _controller.CompleteLogin(_loginToken.Guid);
    }
    
    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<RedirectResult>();
        var redirectResult = (RedirectResult) _actionResult;
        redirectResult.Url.ShouldBe("/");
    }

    [TearDown]
    public async Task TearDown()
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();

        var userRepository = new UserRepository(newUnitOfWork);
        var user = await userRepository.GetAsync(_user.Id);
        if (user != null)
        {
            await userRepository.DeleteAsync(user);
        }
        
        var loginTokenRepository = new LoginTokenRepository(newUnitOfWork);
        var loginToken = await loginTokenRepository.GetAsync(_loginToken.Id);
        if (loginToken != null)
        {
            await loginTokenRepository.DeleteAsync(loginToken);
        }        
    }
    
    private void _BuildEntities()
    {
        _user = new UserBuilder(UnitOfWork).Build();

        _loginToken = new LoginTokenBuilder(UnitOfWork)
            .WithEmail(_user.Email)
            .WithTokenReturnUrl(null)
            .Build();
        _loginToken.Confirm();

    }    
}