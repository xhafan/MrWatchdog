using FakeItEasy;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Infrastructure.ActingUserAccessors;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using Rebus.Bus;
using Rebus.Messages;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.CoreBuses;

[TestFixture]
public class when_sending_command_message : BaseDatabaseTest
{
    private IBus _bus = null!;
    private CoreBus _coreBus = null!;
    private CreateWatchdogCommand _command = null!;

    [SetUp]
    public async Task Context()
    {
        _bus = A.Fake<IBus>();
        
        var actingUserAccessor = A.Fake<IActingUserAccessor>();
        A.CallTo(() => actingUserAccessor.GetActingUserId()).Returns(23);

        _coreBus = new CoreBus(
            _bus,
            new ExistingTransactionJobCreator(UnitOfWork),
            actingUserAccessor
        );
        _command = new CreateWatchdogCommand("watchdog name");
        
        await _coreBus.Send(_command);
    }

    [Test]
    public void command_message_is_sent_over_the_bus()
    {
        A.CallTo(() => _bus.Send(
                A<CreateWatchdogCommand>.That.Matches(p => p.Name == "watchdog name" 
                                                           && !p.Guid.Equals(Guid.Empty)
                                                           && p.ActingUserId == 23),
                A<IDictionary<string, string>>.That.Matches(p => p.ContainsKey(Headers.MessageId) && p[Headers.MessageId] == _command.Guid.ToString())
            )
        ).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task command_job_is_created()
    {
        var job = await new JobRepository(UnitOfWork).GetByGuidAsync(_command.Guid);
        job.ShouldNotBeNull();
        job.Guid.ShouldBe(_command.Guid);
        job.Type.ShouldBe(nameof(CreateWatchdogCommand));
        job.HandlingAttempts.ShouldBeEmpty();
    }
}