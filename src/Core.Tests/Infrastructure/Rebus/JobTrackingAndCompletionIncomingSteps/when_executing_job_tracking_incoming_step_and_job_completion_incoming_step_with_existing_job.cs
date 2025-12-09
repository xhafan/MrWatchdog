using CoreDdd.Nhibernate.UnitOfWorks;
using FakeItEasy;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;
using Rebus.Messages;
using Rebus.Pipeline;
using Rebus.Transport;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.JobTrackingAndCompletionIncomingSteps;

[TestFixture]
public class when_executing_job_tracking_incoming_step_and_job_completion_incoming_step_with_existing_job : BaseDatabaseTest
{
    private CreateScraperCommand _command = null!;
    private Scraper _newScraper = null!;
    private Job _job = null!;
    private User _user = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntitiesInSeparateTransaction();

        var jobTrackingIncomingStep = new JobTrackingIncomingStepBuilder().Build();
        
        var jobCompletionIncomingStep = new JobCompletionIncomingStepBuilder(UnitOfWork).Build();

        var incomingStepContext = new IncomingStepContext(
            new TransportMessage(new Dictionary<string, string>(), []), A.Fake<ITransactionContext>()
        );
        incomingStepContext.Save(new Message(new Dictionary<string, string> {{Headers.MessageId, _command.Guid.ToString()}}, _command));

        await jobTrackingIncomingStep.Process(incomingStepContext, async () =>
        {
            await jobCompletionIncomingStep.Process(incomingStepContext, _next);
        });

        await UnitOfWork.FlushAsync();
        return;

        Task _next()
        {
            _newScraper = new ScraperBuilder(UnitOfWork).Build();
            return Task.CompletedTask;
        }
    }

    [Test]
    public void existing_job_is_fetched_and_completed()
    {
        _job = UnitOfWork.LoadById<Job>(_job.Id);

        _job.CompletedOn.ShouldNotBeNull();
        _job.CompletedOn.Value.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
        _job.NumberOfHandlingAttempts.ShouldBe(1);

        var jobAffectedEntity = _job.AffectedEntities.Single(x => x.EntityName == nameof(Scraper));
        jobAffectedEntity.EntityName.ShouldBe(nameof(Scraper));
        jobAffectedEntity.EntityId.ShouldBe(_newScraper.Id);
        
        var jobHandlingAttempt = _job.HandlingAttempts.ShouldHaveSingleItem();
        jobHandlingAttempt.StartedOn.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
        jobHandlingAttempt.EndedOn.ShouldNotBeNull();
        jobHandlingAttempt.EndedOn.Value.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
    }

    [TearDown]
    public async Task TearDown()
    {
        await UnitOfWork.FlushAsync();
        await UnitOfWork.RollbackAsync();
        UnitOfWork.BeginTransaction();

        await NhibernateUnitOfWorkRunner.RunAsync(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            async newUnitOfWork =>
            {
                await newUnitOfWork.DeleteJobCascade(_job);
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

                _command = new CreateScraperCommand(_user.Id, "scraper name") {Guid = Guid.NewGuid()};

                _job = new JobBuilder(newUnitOfWork)
                    .WithGuid(_command.Guid)
                    .WithType(nameof(CreateScraperCommand))
                    .WithInputData(_command)
                    .WithKind(JobKind.Command)
                    .Build();
            }
        );
    }
}