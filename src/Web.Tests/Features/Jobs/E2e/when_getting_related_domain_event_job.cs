using CoreDdd.Nhibernate.UnitOfWorks;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Jobs.Queries;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogWebPageScrapingDataUpdated;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Jobs;
using System.Net;
using System.Net.Http.Json;

namespace MrWatchdog.Web.Tests.Features.Jobs.E2e;

[TestFixture]
public class when_getting_related_domain_event_job : BaseDatabaseTest
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
    public async Task domain_event_job_data_is_correct()
    {
        var url = JobUrlConstants.GetRelatedDomainEventJobUrlTemplate
            .WithCommandJobGuid(_commandJobGuid)
            .WithDomainEventType(nameof(WatchdogWebPageScrapingDataUpdatedDomainEvent));
        var response = await _webApplicationClient.GetAsync(url);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var jobDto = await response.Content.ReadFromJsonAsync<JobDto>();
        jobDto.ShouldNotBeNull();
        jobDto.Guid.ShouldBe(_domainEventJob.Guid);
    }
    
    [TearDown]
    public async Task TearDown()
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();

        await newUnitOfWork.DeleteJobCascade(_domainEventJob.Guid);
        await newUnitOfWork.DeleteJobCascade(_commandJobGuid);
        
        await newUnitOfWork.DeleteLoginTokenCascade(_loginToken);
        await newUnitOfWork.DeleteUserCascade(_user);
        
        await E2ETestHelper.DeleteMarkLoginTokenAsUsedCommandJob(_loginToken.Guid, newUnitOfWork);
    } 

    private void _BuildEntitiesInSeparateTransaction()
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();

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
}