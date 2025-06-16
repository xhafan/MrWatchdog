using Castle.Windsor;
using CoreDdd.Domain.Events;
using FakeItEasy;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.DomainEventHandlerFactories;

[TestFixture]
public class when_releasing_domain_event_handler
{
    private TestDomainEventHandler _testDomainEventHandler = null!;
    private IWindsorContainer _windsorContainer = null!;

    [SetUp]
    public void Context()
    {
        _testDomainEventHandler = new TestDomainEventHandler();

        _windsorContainer = A.Fake<IWindsorContainer>();
        JobContext.WindsorContainer.Value = _windsorContainer;

        var factory = new DomainEventHandlerFactory();
        
        factory.Release(_testDomainEventHandler);
    }

    [Test]
    public void handlers_are_correctly_resolved()
    {
        A.CallTo(() => _windsorContainer.Release(_testDomainEventHandler)).MustHaveHappenedOnceExactly();
    }

    private record TestDomainEvent : DomainEvent;
    
    private class TestDomainEventHandler : IDomainEventHandler<TestDomainEvent>
    {
        public void Handle(TestDomainEvent domainEvent)
        {
        }
    }
}