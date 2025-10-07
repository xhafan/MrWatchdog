using FakeItEasy;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Rebus.RebusQueueRedirectors;
using MrWatchdog.Core.Messages;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using Rebus.Bus.Advanced;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.SendDomainEventOverMessageBusDomainEventHandlers;

[TestFixture]
public class when_handling_duplicate_domain_event : BaseDatabaseTest
{
    private ISyncBus _bus = null!;
    private TestDomainEvent _testDomainEventOne = null!;
    private TestDomainEvent _testDomainEventTwo = null!;

    [SetUp]
    public void Context()
    {
        var commandJob = new JobBuilder(UnitOfWork).Build();
        JobContext.CommandGuid.Value = commandJob.Guid;
        JobContext.RaisedDomainEvents.Value = [];
        JobContext.RebusHandlingQueue.Value = $"Test{RebusQueues.Main}";

        _testDomainEventOne = new TestDomainEvent(23) {RelatedCommandGuid = commandJob.Guid};
        _testDomainEventTwo = new TestDomainEvent(23) {RelatedCommandGuid = commandJob.Guid};;

        _bus = A.Fake<ISyncBus>();
        var handler = new SendDomainEventOverMessageBusDomainEventHandler<TestDomainEvent>(
            _bus, 
            new ExistingTransactionJobCreator(UnitOfWork),
            new JobContextRebusQueueRedirector()
            );

        handler.Handle(_testDomainEventOne);
        handler.Handle(_testDomainEventTwo);
    }
    
    [Test]
    public void only_the_first_domain_event_is_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(
            _testDomainEventOne,
            A<Dictionary<string, string>>._
        )).MustHaveHappenedOnceExactly();
    }
    
    // ReSharper disable once NotAccessedPositionalProperty.Local
    private record TestDomainEvent(int Data) : DomainEvent;
}