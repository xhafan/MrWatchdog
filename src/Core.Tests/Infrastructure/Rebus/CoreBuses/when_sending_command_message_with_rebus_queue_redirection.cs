using FakeItEasy;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Infrastructure.ActingUserAccessors;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Rebus.RebusQueueRedirectors;
using MrWatchdog.Core.Infrastructure.RequestIdAccessors;
using MrWatchdog.TestsShared;
using Rebus.Bus;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.CoreBuses;

[TestFixture]
public class when_sending_command_message_with_rebus_queue_redirection : BaseDatabaseTest
{
    private IBus _bus = null!;
    private CoreBus _coreBus = null!;
    private CreateWatchdogCommand _command = null!;

    [SetUp]
    public async Task Context()
    {
        _bus = A.Fake<IBus>();
        
        var actingUserAccessor = A.Fake<IActingUserAccessor>();
        var requestIdAccessor = A.Fake<IRequestIdAccessor>();

        var rebusQueueRedirector = A.Fake<IRebusQueueRedirector>();
        A.CallTo(() => rebusQueueRedirector.GetQueueForRedirection()).Returns("AdminBulk");

        _coreBus = new CoreBus(
            _bus,
            new ExistingTransactionJobCreator(UnitOfWork),
            actingUserAccessor,
            requestIdAccessor,
            rebusQueueRedirector
        );
        _command = new CreateWatchdogCommand(UserId: 23, "watchdog name");
        
        await _coreBus.Send(_command);
    }

    [Test]
    public void command_message_is_sent_over_the_bus_with_rebus_queue_redirection()
    {
        A.CallTo(() => _bus.Send(
                A<CreateWatchdogCommand>._,
                A<IDictionary<string, string>>.That.Matches(p => p.ContainsKey(CustomHeaders.QueueForRedirection)
                                                                 && p[CustomHeaders.QueueForRedirection] == "AdminBulk")
            )
        ).MustHaveHappenedOnceExactly();
    }
}