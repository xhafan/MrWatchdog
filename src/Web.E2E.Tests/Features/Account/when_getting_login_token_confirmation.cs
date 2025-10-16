using System.Net;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Web.E2E.Tests.Features.Account;

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
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value
            .GetAsync(AccountUrlConstants.ApiGetLoginTokenConfirmationUrlTemplate.WithLoginTokenGuid(_loginToken.Guid));
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
    
    [Test]
    public async Task login_token_confirmation_with_empty_login_token_guid_cannot_be_retrieved()
    {
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value
            .GetAsync(AccountUrlConstants.ApiGetLoginTokenConfirmationUrlTemplate.WithLoginTokenGuid(Guid.Empty));
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).ShouldContain("must not have the default value");
    }
    
    [Test]
    public async Task login_token_confirmation_without_login_token_guid_cannot_be_retrieved()
    {
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.GetAsync("/api/Login/GetLoginTokenConfirmation");
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