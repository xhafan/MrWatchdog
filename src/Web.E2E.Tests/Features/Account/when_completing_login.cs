using System.Net;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using NHibernate;
using NHibernate.Criterion;

namespace MrWatchdog.Web.E2E.Tests.Features.Account;

[TestFixture]
public class when_completing_login : BaseDatabaseTest
{
    private LoginToken _loginToken = null!;
    private User _user = null!;

    [SetUp]
    public void Context()
    {
        _BuildEntitiesInSeparateTransaction();
    }

    [Test]
    public async Task confirm_login_succeeds()
    {
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.PostAsync(
            AccountUrlConstants.ApiCompleteLoginUrlTemplate.WithLoginTokenGuid(_loginToken.Guid), content: null);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).ShouldBe(LoginTokenBuilder.TokenReturnUrl);
    }
    
    [Test]
    public async Task confirm_login_with_empty_login_token_guid_fails()
    {
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.PostAsync(
            AccountUrlConstants.ApiCompleteLoginUrlTemplate.WithLoginTokenGuid(Guid.Empty), content: null);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).ShouldContain("must not have the default value");
    }    
    
    [Test]
    public async Task confirm_login_with_missing_login_token_guid_fails()
    {
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.PostAsync("/api/CompleteLogin/CompleteLogin", content: null);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).ShouldContain("required");
    }  
    
    [TearDown]
    public async Task TearDown()
    {
        await NhibernateUnitOfWorkRunner.RunAsync(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            async newUnitOfWork =>
            {
                await newUnitOfWork.DeleteLoginTokenCascade(_loginToken);
                await newUnitOfWork.DeleteUserCascade(_user);

                var job = await newUnitOfWork.Session!.QueryOver<Job>()
                    .Where(x => x.Type == nameof(MarkLoginTokenAsUsedCommand))
                    .And(Expression.Sql(
                        """
                        ({alias}."InputData" ->> 'loginTokenGuid') = ?
                        """,
                        _loginToken.Guid.ToString(),
                        NHibernateUtil.String)
                    )
                    .SingleOrDefaultAsync();
                await newUnitOfWork.DeleteJobCascade(job);
            }
        );
    }

    private void _BuildEntitiesInSeparateTransaction()
    {
        NhibernateUnitOfWorkRunner.Run(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            newUnitOfWork =>
            {
                _user = new UserBuilder(newUnitOfWork).Build();

                _loginToken = new LoginTokenBuilder(newUnitOfWork)
                    .WithEmail(_user.Email)
                    .Build();
                _loginToken.Confirm();
            }
        );
    }
}