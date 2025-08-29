using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using NHibernate;
using NHibernate.Criterion;
using System.Net;

namespace MrWatchdog.Web.Tests.Features.Account.E2e;

[TestFixture]
public class when_completing_login : BaseDatabaseTest
{
    private LoginToken _loginToken = null!;
    private User _user = null!;

    [SetUp]
    public void Context()
    {
        _user = new UserBuilder(UnitOfWork).Build();
        
        _loginToken = new LoginTokenBuilder(UnitOfWork)
            .WithEmail(_user.Email)
            .Build();
        _loginToken.Confirm();
        
        UnitOfWork.Commit();
        UnitOfWork.BeginTransaction();
    }    
    
    [Test]
    public async Task confirm_login_succeeds()
    {
        var response = await RunOncePerTestRun.WebApplicationClient.Value.PostAsync(
            AccountUrlConstants.ApiCompleteLoginUrl.Replace(AccountUrlConstants.LoginTokenGuidVariable, _loginToken.Guid.ToString()), content: null);
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.ToString().ShouldBe(LoginTokenBuilder.TokenReturnUrl);
    }
    
    [Test]
    public async Task confirm_login_with_empty_login_token_guid_fails()
    {
        var response = await RunOncePerTestRun.WebApplicationClient.Value.PostAsync(
            AccountUrlConstants.ApiCompleteLoginUrl.Replace(AccountUrlConstants.LoginTokenGuidVariable, Guid.Empty.ToString()), content: null);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).ShouldContain("must not have the default value");
    }    
    
    [Test]
    public async Task confirm_login_with_missing_login_token_guid_fails()
    {
        var response = await RunOncePerTestRun.WebApplicationClient.Value.PostAsync("/api/CompleteLogin", content: null);
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

        var userRepository = new UserRepository(UnitOfWork);
        var user = await userRepository.GetAsync(_user.Id);
        if (user != null)
        {
            await userRepository.DeleteAsync(user);
        }        
        
        var job = await UnitOfWork.Session!.QueryOver<Job>()
            .Where(x => x.Type == nameof(MarkLoginTokenAsUsedCommand))
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