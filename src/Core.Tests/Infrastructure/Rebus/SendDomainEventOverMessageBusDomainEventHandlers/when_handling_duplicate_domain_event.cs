using FakeItEasy;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Messages;
using Rebus.Bus.Advanced;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.SendDomainEventOverMessageBusDomainEventHandlers;

[TestFixture]
public class when_handling_duplicate_domain_event
{
    private ISyncBus _bus = null!;
    private TestDomainEvent _testDomainEventOne = null!;
    private TestDomainEvent _testDomainEventTwo = null!;

    [SetUp]
    public void Context()
    {
        JobContext.RaisedDomainEvents.Value = [];
        
        _testDomainEventOne = new TestDomainEvent(23);
        _testDomainEventTwo = new TestDomainEvent(23);

        _bus = A.Fake<ISyncBus>();
        var handler = new SendDomainEventOverMessageBusDomainEventHandler<TestDomainEvent>(_bus);

        handler.Handle(_testDomainEventOne);
        handler.Handle(_testDomainEventTwo);
    }
    
    [Test]
    public void first_domain_event_is_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(
            _testDomainEventOne,
            null
        )).MustHaveHappenedOnceExactly();
    }
    
    // ReSharper disable once NotAccessedPositionalProperty.Local
    private record TestDomainEvent(int Data) : DomainEvent;
}