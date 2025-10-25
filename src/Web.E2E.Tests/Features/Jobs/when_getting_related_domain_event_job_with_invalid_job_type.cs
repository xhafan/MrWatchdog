using CoreDdd.Nhibernate.UnitOfWorks;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogWebPageScrapingDataUpdated;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Jobs;
using System.Net;

namespace MrWatchdog.Web.E2E.Tests.Features.Jobs;

[TestFixture]
public class when_getting_related_domain_event_job_with_invalid_job_type : BaseDatabaseTest
{
    private readonly Guid _commandJobGuid = Guid.NewGuid();
    private Job _commandJob = null!;
    private Job _domainEventJob = null!;
    private HttpClient _webApplicationClient = null!;
    private User _user = null!;
    private LoginToken _loginToken = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntitiesInSeparateTransaction();
        
        _webApplicationClient = RunOncePerTestRun.WebApplicationFactory.Value.CreateDefaultClient(new CookieContainerHandler(new CookieContainer()));
        await E2ETestHelper.LogUserIn(_webApplicationClient, _loginToken.Guid);
    }

    [Test]
    public async Task getting_related_domain_event_data_fails()
    {
        var url = JobUrlConstants.GetRelatedDomainEventJobUrlTemplate
            .WithCommandJobGuid(_commandJobGuid)
            .WithDomainEventType(new string('x', 500));
        var response = await _webApplicationClient.GetAsync(url);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
    
    [TearDown]
    public async Task TearDown()
    {
        await NhibernateUnitOfWorkRunner.RunAsync(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            async newUnitOfWork =>
            {
                await newUnitOfWork.DeleteJobCascade(_domainEventJob.Guid);
                await newUnitOfWork.DeleteJobCascade(_commandJobGuid);

                await newUnitOfWork.DeleteLoginTokenCascade(_loginToken);
                await newUnitOfWork.DeleteUserCascade(_user);

                await E2ETestHelper.DeleteMarkLoginTokenAsUsedCommandJob(_loginToken.Guid, newUnitOfWork);
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

                _commandJob = new JobBuilder(newUnitOfWork)
                    .WithGuid(_commandJobGuid)
                    .Build();

                _domainEventJob = new JobBuilder(newUnitOfWork)
                    .WithType(nameof(WatchdogWebPageScrapingDataUpdatedDomainEvent))
                    .Build();
                _domainEventJob.SetRelatedCommandJob(_commandJob);
            }
        );
    }
}