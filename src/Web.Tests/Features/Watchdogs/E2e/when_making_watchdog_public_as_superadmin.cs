using System.Net;
using CoreDdd.Nhibernate.UnitOfWorks;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.E2e;

[TestFixture]
public class when_making_watchdog_public_as_superadmin : BaseDatabaseTest
{
    private LoginToken _loginToken = null!;
    private User _superAdminUser = null!;
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
    public async Task superadmin_user_is_allowed_making_watchdog_public()
    {
        var response = await _webApplicationClient.GetAsync(
            WatchdogUrlConstants.WatchdogDetailActionsUrlTemplate.WithWatchdogId(_watchdog.Id));
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var pageHtml = await response.Content.ReadAsStringAsync();

        response = await _webApplicationClient.PostAsync(
            WatchdogUrlConstants.WatchdogDetailActionsMakePublicUrlTemplate.WithWatchdogId(_watchdog.Id),
            content: E2ETestHelper.GetFormUrlEncodedContentWithRequestVerificationToken(E2ETestHelper.ExtractRequestVerificationToken(pageHtml))
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [TearDown]
    public async Task TearDown()
    {
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        _webApplicationClient?.Dispose();
        
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();

        await E2ETestHelper.DeleteMarkLoginTokenAsUsedCommandJob(_loginToken.Guid, newUnitOfWork);
        await E2ETestHelper.DeleteWatchdogCommandJob<MakeWatchdogPublicCommand>(_watchdog.Id, newUnitOfWork);
        
        await newUnitOfWork.DeleteLoginTokenCascade(_loginToken);
        await newUnitOfWork.DeleteUserCascade(_superAdminUser);
        await newUnitOfWork.DeleteWatchdogCascade(_watchdog);
    }

    private void _BuildEntitiesInSeparateTransaction()
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();

        _superAdminUser = new UserBuilder(newUnitOfWork)
            .WithSuperAdmin(true)
            .Build();
        
        _loginToken = new LoginTokenBuilder(newUnitOfWork)
            .WithEmail(_superAdminUser.Email)
            .Build();
        _loginToken.Confirm();

        _watchdog = new WatchdogBuilder(newUnitOfWork).Build();
    }
}