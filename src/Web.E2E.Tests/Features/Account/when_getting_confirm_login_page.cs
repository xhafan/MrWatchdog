using System.Net;
using CoreBackend.Account.Features.Account;
using CoreBackend.Account.Features.Account.Commands;
using CoreBackend.Account.Features.Account.Domain;
using CoreBackend.Features.Jobs.Domain;
using CoreBackend.TestsShared;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.TestsShared;
using MrWatchdog.Core.TestsShared.Builders;
using NHibernate;
using NHibernate.Criterion;

namespace MrWatchdog.Web.E2E.Tests.Features.Account;

[TestFixture]
public class when_getting_confirm_login_page : BaseDatabaseTest
{
    private LoginToken _loginToken = null!;

    [SetUp]
    public void Context()
    {
        _BuildEntitiesInSeparateTransaction();
    }    
    
    [Test]
    public async Task confirm_login_page_can_be_fetched()
    {
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value
            .GetAsync(AccountUrlConstants.AccountConfirmLoginUrlTemplate.WithLoginToken(_loginToken.Token));
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
    
    [Test]
    public async Task confirm_login_page_with_missing_token_cannot_be_fetched()
    {
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.GetAsync("/Account/ConfirmLogin");
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

                var job = await newUnitOfWork.Session!.QueryOver<Job>()
                    .Where(x => x.Type == nameof(ConfirmLoginTokenCommand))
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
                _loginToken = new LoginTokenBuilder(newUnitOfWork).Build();
            }
        );
    }
}