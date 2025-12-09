using System.Net;
using CoreDdd.Nhibernate.UnitOfWorks;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Scrapers;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using NHibernate;
using NHibernate.Criterion;

namespace MrWatchdog.Web.E2E.Tests.Features.Scrapers;

[TestFixture]
public class when_making_scraper_public_as_non_superadmin : BaseDatabaseTest
{
    private LoginToken _loginToken = null!;
    private User _nonSuperAdminUser = null!;
    private Scraper _scraper = null!;
    private HttpClient _webApplicationClient = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntitiesInSeparateTransaction();

        _webApplicationClient = RunOncePerTestRun.WebApplicationFactory.Value.CreateDefaultClient(new CookieContainerHandler(new CookieContainer()));
        await E2ETestHelper.LogUserIn(_webApplicationClient, _loginToken.Guid);
    }

    [Test]
    public async Task non_superadmin_user_is_denied_making_scraper_public()
    {
        var response = await _webApplicationClient.GetAsync(
            ScraperUrlConstants.ScraperDetailActionsUrlTemplate.WithScraperId(_scraper.Id));
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var pageHtml = await response.Content.ReadAsStringAsync();

        response = await _webApplicationClient.PostAsync(
            ScraperUrlConstants.ScraperDetailActionsMakePublicUrlTemplate.WithScraperId(_scraper.Id),
            content: E2ETestHelper.GetFormUrlEncodedContentWithRequestVerificationToken(E2ETestHelper.ExtractRequestVerificationToken(pageHtml))
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
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

                var makeScraperPublicCommandJob = await newUnitOfWork.Session!.QueryOver<Job>()
                    .Where(x => x.Type == nameof(MakeScraperPublicCommand))
                    .And(Expression.Sql(
                        """
                        ({alias}."InputData" ->> 'scraperId') = ?
                        """,
                        _scraper.Id.ToString(),
                        NHibernateUtil.String)
                    )
                    .SingleOrDefaultAsync();
                await newUnitOfWork.DeleteJobCascade(makeScraperPublicCommandJob, waitForJobCompletion: true);

                await newUnitOfWork.DeleteLoginTokenCascade(_loginToken);
                await newUnitOfWork.DeleteScraperCascade(_scraper);
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

                _scraper = new ScraperBuilder(newUnitOfWork)
                    .WithUser(_nonSuperAdminUser)
                    .Build();
            }
        );
    }
}