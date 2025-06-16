using FakeItEasy;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Messages;
using Rebus.Bus.Advanced;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.SendDomainEventOverMessageBusDomainEventHandlers;

[TestFixture]
public class when_handling_domain_event
{
    private ISyncBus _bus = null!;
    private TestDomainEvent _testDomainEvent = null!;
    private readonly Guid _commandGuid = Guid.NewGuid();

    [SetUp]
    public void Context()
    {
        JobContext.RaisedDomainEvents.Value = [];
        JobContext.CommandGuid.Value = _commandGuid;
        _testDomainEvent = new TestDomainEvent();

        _bus = A.Fake<ISyncBus>();
        var handler = new SendDomainEventOverMessageBusDomainEventHandler<TestDomainEvent>(_bus);

        handler.Handle(_testDomainEvent);
    }
    
    [Test]
    public void domain_event_is_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(
            _testDomainEvent,
            null
        )).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void related_command_guid_is_set_on_domain_event()
    {
        _testDomainEvent.RelatedCommandGuid.ShouldBe(_commandGuid);
    }
    
    private record TestDomainEvent : DomainEvent;
}