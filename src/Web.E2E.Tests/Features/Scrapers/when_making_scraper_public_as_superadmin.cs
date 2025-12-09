using System.Net;
using CoreDdd.Nhibernate.UnitOfWorks;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Web.E2E.Tests.Features.Scrapers;

[TestFixture]
public class when_making_scraper_public_as_superadmin : BaseDatabaseTest
{
    private LoginToken _loginToken = null!;
    private User _superAdminUser = null!;
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
    public async Task superadmin_user_is_allowed_making_scraper_public()
    {
        var response = await _webApplicationClient.GetAsync(
            ScraperUrlConstants.ScraperDetailActionsUrlTemplate.WithScraperId(_scraper.Id));
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var pageHtml = await response.Content.ReadAsStringAsync();

        response = await _webApplicationClient.PostAsync(
            ScraperUrlConstants.ScraperDetailActionsMakePublicUrlTemplate.WithScraperId(_scraper.Id),
            content: E2ETestHelper.GetFormUrlEncodedContentWithRequestVerificationToken(E2ETestHelper.ExtractRequestVerificationToken(pageHtml))
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
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
                await E2ETestHelper.DeleteScraperCommandJob<MakeScraperPublicCommand>(_scraper.Id, newUnitOfWork);

                await newUnitOfWork.DeleteLoginTokenCascade(_loginToken);
                await newUnitOfWork.DeleteUserCascade(_superAdminUser);
                await newUnitOfWork.DeleteScraperCascade(_scraper);
            }
        );
    }

    private void _BuildEntitiesInSeparateTransaction()
    {
        NhibernateUnitOfWorkRunner.Run(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            newUnitOfWork =>
            {
                _superAdminUser = new UserBuilder(newUnitOfWork)
                    .WithSuperAdmin(true)
                    .Build();

                _loginToken = new LoginTokenBuilder(newUnitOfWork)
                    .WithEmail(_superAdminUser.Email)
                    .Build();
                _loginToken.Confirm();

                _scraper = new ScraperBuilder(newUnitOfWork).Build();
            }
        );
    }
}