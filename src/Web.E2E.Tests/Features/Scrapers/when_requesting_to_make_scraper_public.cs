using CoreBackend.Features.Jobs.Domain;
using CoreBackend.Features.Jobs.Services;
using CoreBackend.Infrastructure.Rebus;
using CoreBackend.Infrastructure.Repositories;
using CoreDdd.Nhibernate.UnitOfWorks;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperRequestedToBeMadePublic;
using System.Net;
using CoreBackend.Account.Features.Account.Domain;
using CoreBackend.TestsShared;
using MrWatchdog.Core.TestsShared;
using MrWatchdog.Core.TestsShared.Builders;

namespace MrWatchdog.Web.E2E.Tests.Features.Scrapers;

[TestFixture]
public class when_requesting_to_make_scraper_public : BaseDatabaseTest
{
    private LoginToken _loginToken = null!;
    private User _user = null!;
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
    public async Task domain_event_job_is_successfully_created()
    {
        var response = await _webApplicationClient.GetAsync(
            ScraperUrlConstants.ScraperDetailActionsUrlTemplate.WithScraperId(_scraper.Id));
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var pageHtml = await response.Content.ReadAsStringAsync();

        response = await _webApplicationClient.PostAsync(
            ScraperUrlConstants.ScraperDetailActionsRequestToMakePublicUrlTemplate.WithScraperId(_scraper.Id),
            content: E2ETestHelper.GetFormUrlEncodedContentWithRequestVerificationToken(E2ETestHelper.ExtractRequestVerificationToken(pageHtml))
        );

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var requestToMakeScraperPublicCommandJobGuid = Guid.Parse(await response.Content.ReadAsStringAsync());
        var jobAwaiter = new JobCompletionAwaiter(TestFixtureContext.NhibernateConfigurator, OptionsTestRetriever.Retrieve<RebusOptions>());
        await jobAwaiter.WaitForJobCompletion(requestToMakeScraperPublicCommandJobGuid);

        var jobRepository = new JobRepository(UnitOfWork);
        var requestToMakeScraperPublicCommandJob = await jobRepository.GetByGuidAsync(requestToMakeScraperPublicCommandJobGuid);
        var scraperRequestedToBeMadePublicDomainEventJob = await UnitOfWork.Session!.QueryOver<Job>()
            .Where(x => x.RelatedCommandJob == requestToMakeScraperPublicCommandJob)
            .SingleOrDefaultAsync();
        scraperRequestedToBeMadePublicDomainEventJob.ShouldNotBeNull();
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
                await E2ETestHelper.DeleteScraperCommandJob<ScraperRequestedToBeMadePublicDomainEvent>(_scraper.Id, newUnitOfWork);
                await E2ETestHelper.DeleteScraperCommandJob<RequestToMakeScraperPublicCommand>(_scraper.Id, newUnitOfWork);
                await E2ETestHelper.DeleteSendEmailCommandJob("requested to be made public", newUnitOfWork);

                await newUnitOfWork.DeleteLoginTokenCascade(_loginToken);
                await newUnitOfWork.DeleteScraperCascade(_scraper);
                await newUnitOfWork.DeleteUserCascade(_user);
            }
        );
    }

    private void _BuildEntitiesInSeparateTransaction()
    {
        NhibernateUnitOfWorkRunner.Run(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            newUnitOfWork =>
            {
                _user = new UserBuilder(newUnitOfWork).Build();

                _loginToken = new LoginTokenBuilder(newUnitOfWork)
                    .WithEmail(_user.Email)
                    .Build();
                _loginToken.Confirm();

                _scraper = new ScraperBuilder(newUnitOfWork)
                    .WithUser(_user)
                    .Build();
            }
        );
    }
}