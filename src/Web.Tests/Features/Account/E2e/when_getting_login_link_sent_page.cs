using System.Net;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Web.Tests.Features.Account.E2e;

[TestFixture]
public class when_getting_login_link_sent_page : BaseDatabaseTest
{
    private LoginToken _loginToken = null!;

    [SetUp]
    public void Context()
    {
        _BuildEntitiesInSeparateTransaction();
    }

    [Test]
    public async Task login_link_page_can_be_fetched()
    {
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value
            .GetAsync(AccountUrlConstants.AccountLoginLinkSentUrlTemplate.WithLoginTokenGuid(_loginToken.Guid));
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
    
    [Test]
    public async Task login_link_page_with_missing_login_token_guid_cannot_be_fetched()
    {
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.GetAsync("/Account/LoginLinkSent");
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).ShouldContain("required");
    }     
    
    [TearDown]
    public async Task TearDown()
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();

        await newUnitOfWork.DeleteLoginTokenCascade(_loginToken);
    }

    private void _BuildEntitiesInSeparateTransaction()
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();

        _loginToken = new LoginTokenBuilder(newUnitOfWork).Build();
    }
}