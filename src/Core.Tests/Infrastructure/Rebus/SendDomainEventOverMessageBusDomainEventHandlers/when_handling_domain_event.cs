using FakeItEasy;
using MrWatchdog.Core.Features.Jobs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Rebus.RebusQueueRedirectors;
using MrWatchdog.Core.Messages;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using Rebus.Bus.Advanced;
using Rebus.Messages;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.SendDomainEventOverMessageBusDomainEventHandlers;

[TestFixture]
public class when_handling_domain_event : BaseDatabaseTest
{
    private ISyncBus _bus = null!;
    private TestDomainEvent _testDomainEvent = null!;
    private readonly Guid _commandGuid = Guid.NewGuid();

    [SetUp]
    public void Context()
    {
        // ReSharper disable once UnusedVariable
        var commandJob = new JobBuilder(UnitOfWork)
            .WithGuid(_commandGuid)
            .Build();

        JobContext.RaisedDomainEvents.Value = [];
        JobContext.CommandGuid.Value = _commandGuid;
        JobContext.ActingUserId.Value = 23;
        JobContext.RequestId.Value = "0HNFBP8T98MQS:00000045";
        JobContext.RebusHandlingQueue.Value = $"Test{RebusQueues.Scraping}";

        _testDomainEvent = new TestDomainEvent {RelatedCommandGuid = _commandGuid};

        _bus = A.Fake<ISyncBus>();
        var handler = new SendDomainEventOverMessageBusDomainEventHandler<TestDomainEvent>(
            _bus, 
            new ExistingTransactionJobCreator(UnitOfWork),
            new JobContextRebusQueueRedirector()
            );

        handler.Handle(_testDomainEvent);
    }
    
    [Test]
    public void domain_event_is_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(
            A<object>.That.Matches(p => _MatchingDomainEvent(p)),
            A<Dictionary<string, string>>.That.Matches(p => _MatchingDomainEventMessageHeaders(p))
        )).MustHaveHappenedOnceExactly();
    }

    private bool _MatchingDomainEvent(object message)
    {
        message.ShouldBeOfType<TestDomainEvent>();
        var testDomainEvent = (TestDomainEvent) message;
        testDomainEvent.RelatedCommandGuid.ShouldBe(_commandGuid);
        testDomainEvent.ActingUserId.ShouldBe(23);
        testDomainEvent.RequestId.ShouldBe("0HNFBP8T98MQS:00000045");

        return true;
    }

    private bool _MatchingDomainEventMessageHeaders(Dictionary<string, string> domainEventMessageHeaders)
    {
        domainEventMessageHeaders.ShouldNotBeNull();

        domainEventMessageHeaders.ShouldContainKey(Headers.MessageId);
        domainEventMessageHeaders[Headers.MessageId].ShouldMatch("[0-9A-Fa-f\\-]{36}");

        domainEventMessageHeaders.ShouldContainKey(CustomHeaders.QueueForRedirection);
        domainEventMessageHeaders[CustomHeaders.QueueForRedirection].ShouldBe($"Test{RebusQueues.Scraping}{RebusConstants.RebusSendQueueSuffix}");

        return true;
    }    

    [Test]
    public void related_command_guid_is_set_on_domain_event()
    {
        _testDomainEvent.RelatedCommandGuid.ShouldBe(_commandGuid);
    }
    
    [Test]
    public async Task domain_event_job_is_created()
    {
        var jobDtos = await new GetRelatedDomainEventJobQueryHandler(UnitOfWork)
            .ExecuteAsync<JobDto>(new GetRelatedDomainEventJobQuery(_commandGuid, nameof(TestDomainEvent)));
        var jobDto = jobDtos.ShouldHaveSingleItem();
        jobDto.HandlingAttempts.ShouldBeEmpty();
    }    
    
    private record TestDomainEvent : DomainEvent;
}