using System.Net;
using CoreDdd.Nhibernate.UnitOfWorks;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Web.E2E.Tests.Features.Watchdogs;

[TestFixture]
public class when_getting_manage_other_users_watchdogs_as_non_superadmin : BaseDatabaseTest
{
    private LoginToken _loginToken = null!;
    private User _nonSuperAdminUser = null!;
    private Watchdog _watchdog = null!;
    private HttpClient _webApplicationClient = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntitiesInSeparateTransaction();

        _webApplicationClient = RunOncePerTestRun.WebApplicationFactory.Value.CreateDefaultClient(new CookieContainerHandler(new CookieContainer()));
        await E2ETestHelper.LogUserIn(_webApplicationClient, _loginToken.Guid);
    }

    [Test]
    public async Task non_superadmin_user_cannot_view_manage_other_users_watchdogs()
    {
        var response = await _webApplicationClient.GetAsync(WatchdogUrlConstants.WatchdogsManageOtherUsersWatchdogsUrl);
        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location.ShouldNotBeNull();
        response.Headers.Location.ToString().ShouldEndWith("/Account/AccessDenied?ReturnUrl=%2FWatchdogs%2FManage%2FOtherUsersWatchdogs");
    }

    [TearDown]
    public async Task TearDown()
    {
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        _webApplicationClient?.Dispose();

        await NhibernateUnitOfWorkRunner.RunAsync(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            async newUnitOfWork =>
            {
                await E2ETestHelper.DeleteMarkLoginTokenAsUsedCommandJob(_loginToken.Guid, newUnitOfWork);

                await newUnitOfWork.DeleteLoginTokenCascade(_loginToken);
                await newUnitOfWork.DeleteUserCascade(_nonSuperAdminUser);
                await newUnitOfWork.DeleteWatchdogCascade(_watchdog);
            }
        );
    }

    private void _BuildEntitiesInSeparateTransaction()
    {
        NhibernateUnitOfWorkRunner.Run(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            newUnitOfWork =>
            {
                _nonSuperAdminUser = new UserBuilder(newUnitOfWork)
                    .WithSuperAdmin(false)
                    .Build();

                _loginToken = new LoginTokenBuilder(newUnitOfWork)
                    .WithEmail(_nonSuperAdminUser.Email)
                    .Build();
                _loginToken.Confirm();

                _watchdog = new WatchdogBuilder(newUnitOfWork).Build();
            }
        );
    }
}