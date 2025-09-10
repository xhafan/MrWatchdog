using System.Net;
using CoreDdd.Nhibernate.UnitOfWorks;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using NHibernate;
using NHibernate.Criterion;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.E2e;

[TestFixture]
public class when_making_watchdog_public_as_superadmin : BaseDatabaseTest
{
    private LoginToken _loginToken = null!;
    private User _nonSuperAdminUser = null!;
    private Watchdog _watchdog = null!;
    private HttpClient _webApplicationClient = null!;

    [SetUp]
    public void Context()
    {
        _BuildEntitiesInSeparateTransaction();

        _webApplicationClient = RunOncePerTestRun.WebApplicationFactory.Value.CreateDefaultClient(new CookieContainerHandler(new CookieContainer()));
    }

    [Test]
    public async Task super_admin_user_is_allowed_making_watchdog_public()
    {
        await E2ETestHelper.LogUserIn(_webApplicationClient, _loginToken.Guid);

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

        var markLoginTokenAsUsedCommandJob = await newUnitOfWork.Session!.QueryOver<Job>()
            .Where(x => x.Type == nameof(MarkLoginTokenAsUsedCommand))
            .And(Expression.Sql(
                """
                ({alias}."InputData" ->> 'loginTokenGuid') = ?
                """,
                _loginToken.Guid.ToString(),
                NHibernateUtil.String)
            )
            .SingleOrDefaultAsync();
        await newUnitOfWork.DeleteJobCascade(markLoginTokenAsUsedCommandJob, waitForJobCompletion: true);
        
        var makeWatchdogPublicCommandJob = await newUnitOfWork.Session!.QueryOver<Job>()
            .Where(x => x.Type == nameof(MakeWatchdogPublicCommand))
            .And(Expression.Sql(
                """
                ({alias}."InputData" ->> 'watchdogId') = ?
                """,
                _watchdog.Id.ToString(),
                NHibernateUtil.String)
            )
            .SingleOrDefaultAsync();
        await newUnitOfWork.DeleteJobCascade(makeWatchdogPublicCommandJob, waitForJobCompletion: true); 
        
        await newUnitOfWork.DeleteLoginTokenCascade(_loginToken);
        await newUnitOfWork.DeleteUserCascade(_nonSuperAdminUser);
        await newUnitOfWork.DeleteWatchdogCascade(_watchdog);
    }

    private void _BuildEntitiesInSeparateTransaction()
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();

        _nonSuperAdminUser = new UserBuilder(newUnitOfWork)
            .WithSuperAdmin(true)
            .Build();
        
        _loginToken = new LoginTokenBuilder(newUnitOfWork)
            .WithEmail(_nonSuperAdminUser.Email)
            .Build();
        _loginToken.Confirm();

        _watchdog = new WatchdogBuilder(newUnitOfWork).Build();
    }
}