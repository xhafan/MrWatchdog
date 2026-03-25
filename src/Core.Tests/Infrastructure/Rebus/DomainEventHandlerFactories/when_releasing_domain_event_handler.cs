using CoreBackend.Infrastructure.Rebus;
using CoreBackend.Messages;
using CoreDdd.Domain.Events;
using CoreIoC;
using FakeItEasy;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.DomainEventHandlerFactories;

[TestFixture]
public class when_releasing_domain_event_handler
{
    private TestDomainEventHandler _testDomainEventHandler = null!;
    private IContainer _ioCContainer = null!;

    [SetUp]
    public void Context()
    {
        _testDomainEventHandler = new TestDomainEventHandler();

        _ioCContainer = A.Fake<IContainer>();
        JobContext.IoCContainer.Value = new FlowContext<IContainer?> {Value = _ioCContainer};

        var factory = new DomainEventHandlerFactory();
        
        factory.Release(_testDomainEventHandler);
    }

    [Test]
    public void handlers_are_correctly_released()
    {
        A.CallTo(() => _ioCContainer.Release(_testDomainEventHandler)).MustHaveHappenedOnceExactly();
    }

    private record TestDomainEvent : DomainEvent;
    
    private class TestDomainEventHandler : IDomainEventHandler<TestDomainEvent>
    {
        public void Handle(TestDomainEvent domainEvent)
        {
        }
    }
}