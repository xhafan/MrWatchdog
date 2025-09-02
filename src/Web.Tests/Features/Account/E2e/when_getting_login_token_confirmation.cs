using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using System.Net;

namespace MrWatchdog.Web.Tests.Features.Account.E2e;

[TestFixture]
public class when_getting_login_token_confirmation : BaseDatabaseTest
{
    private LoginToken _loginToken = null!;

    [SetUp]
    public void Context()
    {
        _BuildEntitiesInSeparateTransaction();
    } 
    
    [Test]
    public async Task login_token_confirmation_can_be_retrieved()
    {
        var response = await RunOncePerTestRun.WebApplicationClient.Value
            .GetAsync(AccountUrlConstants.ApiGetLoginTokenConfirmationUrl.Replace(AccountUrlConstants.LoginTokenGuidVariable, _loginToken.Guid.ToString()));
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
    
    [Test]
    public async Task login_token_confirmation_with_empty_login_token_guid_cannot_be_retrieved()
    {
        var response = await RunOncePerTestRun.WebApplicationClient.Value
            .GetAsync(AccountUrlConstants.ApiGetLoginTokenConfirmationUrl.Replace(AccountUrlConstants.LoginTokenGuidVariable, Guid.Empty.ToString()));
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).ShouldContain("must not have the default value");
    }
    
    [Test]
    public async Task login_token_confirmation_without_login_token_guid_cannot_be_retrieved()
    {
        var response = await RunOncePerTestRun.WebApplicationClient.Value.GetAsync("/api/Login/GetLoginTokenConfirmation");
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