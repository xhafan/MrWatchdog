﻿using FakeItEasy;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Infrastructure.Rebus;
using Rebus.Bus;
using Rebus.Messages;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.CoreBuses;

[TestFixture]
public class when_sending_command_message
{
    private IBus _bus = null!;
    private CoreBus _coreBus = null!;
    private CreateWatchdogCommand _command = null!;

    [SetUp]
    public async Task Context()
    {
        _bus = A.Fake<IBus>();
        _coreBus = new CoreBus(_bus);
        _command = new CreateWatchdogCommand("watchdog name");
        
        await _coreBus.Send(_command);
    }

    [Test]
    public void command_message_is_sent_over_the_bus()
    {
        A.CallTo(() => _bus.Send(
                A<CreateWatchdogCommand>.That.Matches(p => p.Name == "watchdog name" 
                                                           && !p.Guid.Equals(Guid.Empty)),
                A<IDictionary<string, string>>.That.Matches(p => p.ContainsKey(Headers.MessageId) && p[Headers.MessageId] == _command.Guid.ToString())
            )
        ).MustHaveHappenedOnceExactly();
    }
}