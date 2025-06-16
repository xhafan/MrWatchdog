using Castle.Windsor;
using CoreDdd.Domain.Events;
using FakeItEasy;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Messages;

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

        var windsorContainer = A.Fake<IWindsorContainer>();
        A.CallTo(() => windsorContainer.ResolveAll<IDomainEventHandler<TestDomainEvent>>()).Returns([_testDomainEventHandler]);
        JobContext.WindsorContainer.Value = windsorContainer;

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