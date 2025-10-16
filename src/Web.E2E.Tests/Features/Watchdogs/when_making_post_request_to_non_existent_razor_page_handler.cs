﻿using System.Net;
using CoreDdd.Nhibernate.UnitOfWorks;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using NHibernate;
using NHibernate.Criterion;

namespace MrWatchdog.Web.E2E.Tests.Features.Watchdogs;

[TestFixture]
public class when_making_post_request_to_non_existent_razor_page_handler : BaseDatabaseTest
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
    public async Task not_found_results_is_returned()
    {
        var response = await _webApplicationClient.GetAsync(
            WatchdogUrlConstants.WatchdogDetailActionsUrlTemplate.WithWatchdogId(_watchdog.Id));
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var pageHtml = await response.Content.ReadAsStringAsync();

        response = await _webApplicationClient.PostAsync(
            $"/Watchdogs/Detail/Actions/{_watchdog.Id}?handler=NonExistentAction",
            content: E2ETestHelper.GetFormUrlEncodedContentWithRequestVerificationToken(E2ETestHelper.ExtractRequestVerificationToken(pageHtml))
        );

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
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

                await newUnitOfWork.DeleteLoginTokenCascade(_loginToken);
                await newUnitOfWork.DeleteWatchdogCascade(_watchdog);
                await newUnitOfWork.DeleteUserCascade(_nonSuperAdminUser);
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

                _watchdog = new WatchdogBuilder(newUnitOfWork)
                    .WithUser(_nonSuperAdminUser)
                    .Build();
            }
        );
    }
}