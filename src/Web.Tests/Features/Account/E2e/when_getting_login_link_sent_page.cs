using System.Net;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.Core.Infrastructure.Repositories;
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
        _loginToken = new LoginTokenBuilder(UnitOfWork).Build();
        
        UnitOfWork.Commit();
        UnitOfWork.BeginTransaction();
    }    
    
    [Test]
    public async Task login_link_page_can_be_fetched()
    {
        var response = await RunOncePerTestRun.WebApplicationClient.Value
            .GetAsync(AccountUrlConstants.AccountLoginLinkSentUrl.Replace(AccountUrlConstants.LoginTokenGuidVariable, _loginToken.Guid.ToString()));
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
    
    [Test]
    public async Task login_link_page_with_missing_login_token_guid_cannot_be_fetched()
    {
        var response = await RunOncePerTestRun.WebApplicationClient.Value.GetAsync("/Account/LoginLinkSent");
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
        
        await UnitOfWork.CommitAsync();
        UnitOfWork.BeginTransaction();         
    }
}