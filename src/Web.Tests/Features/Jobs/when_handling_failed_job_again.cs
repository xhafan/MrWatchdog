using FakeItEasy;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperScrapingCompleted;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Messages;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Jobs;
using Rebus.Bus;
using Rebus.Messages;

namespace MrWatchdog.Web.Tests.Features.Jobs;

[TestFixture]
public class when_handling_failed_job_again : BaseDatabaseTest
{
    private IBus _rebusBus = null!;
    private JobsController _controller = null!;

    [SetUp]
    public void Context()
    {
        _rebusBus = A.Fake<IBus>();
        
        _controller = new JobsControllerBuilder(UnitOfWork)
            .Build();
    }

    [Test]
    public async Task command_is_sent_over_bus_with_correct_headers()
    {
        var job = new JobBuilder(UnitOfWork)
            .WithGuid(Guid.NewGuid())
            .Build();
        job.HandlingStarted(RebusQueues.Main);

        await _controller.HandleFailedJobAgain(job.Guid, _rebusBus);

        A.CallTo(() => _rebusBus.Send(
                A<Command>.That.Matches(cmd => _MatchesCommand(cmd)),
                A<Dictionary<string, string>>.That.Matches(headers => _MatchesHeaders(headers, job.Guid))
            )
        ).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task domain_event_is_sent_over_bus_with_correct_headers()
    {
        var domainEvent = new ScraperScrapingCompletedDomainEvent(ScraperId: 23)
        {
            RelatedCommandGuid = Guid.NewGuid()
        };
        
        var job = new JobBuilder(UnitOfWork)
            .WithGuid(Guid.NewGuid())
            .WithType(nameof(ScraperScrapingCompletedDomainEvent))
            .WithInputData(domainEvent)
            .WithKind(JobKind.DomainEvent)
            .Build();
        job.HandlingStarted(RebusQueues.Main);

        await _controller.HandleFailedJobAgain(job.Guid, _rebusBus);

        A.CallTo(() => _rebusBus.Send(
                A<DomainEvent>.That.Matches(evt => _MatchesDomainEvent(evt, domainEvent)),
                A<Dictionary<string, string>>.That.Matches(headers => _MatchesHeaders(headers, job.Guid))
            )
        ).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void exception_is_thrown_with_job_is_already_completed()
    {
        var job = new JobBuilder(UnitOfWork)
            .WithGuid(Guid.NewGuid())
            .Build();
        job.HandlingStarted(RebusQueues.Main);

        job.Complete();

        var ex = Should.Throw<Exception>(async () => await _controller.HandleFailedJobAgain(job.Guid, _rebusBus));

        ex.Message.ShouldContain("has been already completed");
    }

    [Test]
    public async Task already_completed_job_can_be_handled_again_with_ignore_completed_job_flag_set()
    {
        var job = new JobBuilder(UnitOfWork)
            .WithGuid(Guid.NewGuid())
            .Build();
        job.HandlingStarted(RebusQueues.Main);
        job.Complete();

        await _controller.HandleFailedJobAgain(job.Guid, _rebusBus, ignoreCompletedJob: true);

        A.CallTo(() => _rebusBus.Send(
                A<Command>._,
                A<Dictionary<string, string>>._
            )
        ).MustHaveHappenedOnceExactly();
    }

    private bool _MatchesCommand(Command command)
    {
        command.ShouldBeOfType<CreateScraperCommand>();
        var createScraperCommand = (CreateScraperCommand)command;
        createScraperCommand.UserId.ShouldBe(23);
        createScraperCommand.Name.ShouldBe("scraper name");
        return true;
    }

    private bool _MatchesDomainEvent(DomainEvent domainEvent, ScraperScrapingCompletedDomainEvent expectedEvent)
    {
        domainEvent.ShouldBeOfType<ScraperScrapingCompletedDomainEvent>();
        var scraperScrapingCompletedDomainEvent = (ScraperScrapingCompletedDomainEvent)domainEvent;
        scraperScrapingCompletedDomainEvent.ScraperId.ShouldBe(expectedEvent.ScraperId);
        scraperScrapingCompletedDomainEvent.RelatedCommandGuid.ShouldBe(expectedEvent.RelatedCommandGuid);
        return true;
    }

    private bool _MatchesHeaders(Dictionary<string, string> headers, Guid expectedJobGuid)
    {
        headers.ShouldNotBeNull();
        headers.ShouldContainKey(Headers.MessageId);
        headers[Headers.MessageId].ShouldBe(expectedJobGuid.ToString());
        return true;
    }
}
