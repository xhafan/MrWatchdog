using CoreBackend.Infrastructure.Rebus;
using CoreBackend.Messages;
using CoreDdd.Domain.Events;
using CoreIoC;
using FakeItEasy;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.DomainEventHandlerFactories;

[TestFixture]
public class when_creating_domain_event_handlers
{
    private IEnumerable<IDomainEventHandler<TestDomainEvent>> _handlers = null!;
    private TestDomainEventHandler _testDomainEventHandler = null!;

    [SetUp]
    public void Context()
    {
        _testDomainEventHandler = new TestDomainEventHandler();

        var ioCContainer = A.Fake<IContainer>();
        A.CallTo(() => ioCContainer.ResolveAll<IDomainEventHandler<TestDomainEvent>>()).Returns([_testDomainEventHandler]);
        JobContext.IoCContainer.Value = new FlowContext<IContainer?> {Value = ioCContainer};

        var factory = new DomainEventHandlerFactory();
        
        _handlers = factory.Create<TestDomainEvent>();
    }

    [Test]
    public void handlers_are_correctly_resolved()
    {
        _handlers.ShouldBe([_testDomainEventHandler]);
    }

    private record TestDomainEvent : DomainEvent;
    
    private class TestDomainEventHandler : IDomainEventHandler<TestDomainEvent>
    {
        public void Handle(TestDomainEvent domainEvent)
        {
        }
    }
}