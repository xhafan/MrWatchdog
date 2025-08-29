using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using System.Net;
using NHibernate;
using NHibernate.Criterion;

namespace MrWatchdog.Web.Tests.Features.Account.E2e;

[TestFixture]
public class when_getting_confirm_login_page : BaseDatabaseTest
{
    private LoginToken _loginToken = null!;

    [SetUp]
    public void Context()
    {
        _loginToken = new LoginTokenBuilder(UnitOfWork).Build();
        
        UnitOfWork.Commit();
        UnitOfWork.BeginTransaction();
    }    
    
    [Test]
    public async Task confirm_login_page_can_be_fetched()
    {
        var response = await RunOncePerTestRun.WebApplicationClient.Value
            .GetAsync(AccountUrlConstants.AccountConfirmLoginUrl.Replace(AccountUrlConstants.TokenVariable, _loginToken.Token));
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
    
    [Test]
    public async Task confirm_login_page_with_missing_token_cannot_be_fetched()
    {
        var response = await RunOncePerTestRun.WebApplicationClient.Value.GetAsync("/Account/ConfirmLogin");
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).ShouldContain("required");
    }     
    
    [TearDown]
    public async Task TearDown()
    {
        var loginTokenRepository = new LoginTokenRepository(UnitOfWork);
        var loginToken = await loginTokenRepository.GetAsync(_loginToken.Id);
        if (loginToken != null)
        {
            await loginTokenRepository.DeleteAsync(loginToken);
        }

        var job = await UnitOfWork.Session!.QueryOver<Job>()
            .Where(x => x.Type == nameof(ConfirmLoginTokenCommand))
            .And(Expression.Sql(
                """
                ({alias}."InputData" ->> 'loginTokenGuid') = ?
                """,
                _loginToken.Guid.ToString(),
                NHibernateUtil.String)
            )
            .SingleOrDefaultAsync();
        if (job != null)
        {
            var jobRepository = new JobRepository(UnitOfWork);
            await jobRepository.DeleteAsync(job);
        }        
        
        await UnitOfWork.CommitAsync();
        UnitOfWork.BeginTransaction();         
    }
}