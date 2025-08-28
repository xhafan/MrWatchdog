using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Account.CompleteLogin;

namespace MrWatchdog.Web.Tests.Features.Account.CompleteLogin;

[TestFixture]
public class when_completing_login_for_existing_user_without_confirmed_login_token : BaseDatabaseTest
{
    private CompleteLoginController _controller = null!;
    private LoginToken _loginToken = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        await UnitOfWork.CommitAsync();
        UnitOfWork.BeginTransaction();
        
        _controller = new CompleteLoginControllerBuilder(UnitOfWork).Build();
    }
    
    [Test]
    public void exception_is_thrown()
    {
        var ex = Should.Throw<Exception>(async () => await _controller.CompleteLogin(_loginToken.Guid));
        
        ex.Message.ShouldContain("Login token has not been confirmed.");
    }

    [TearDown]
    public async Task TearDown()
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();

        var loginTokenRepository = new LoginTokenRepository(newUnitOfWork);
        var loginToken = await loginTokenRepository.GetAsync(_loginToken.Id);
        if (loginToken != null)
        {
            await loginTokenRepository.DeleteAsync(loginToken);
        }        
    }
    
    private void _BuildEntities()
    {
        _loginToken = new LoginTokenBuilder(UnitOfWork).Build();
    }
}