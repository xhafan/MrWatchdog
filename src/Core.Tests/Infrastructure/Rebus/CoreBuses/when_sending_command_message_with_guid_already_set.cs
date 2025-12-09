using FakeItEasy;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Infrastructure.ActingUserAccessors;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Rebus.RebusQueueRedirectors;
using MrWatchdog.Core.Infrastructure.RequestIdAccessors;
using MrWatchdog.TestsShared;
using Rebus.Bus;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.CoreBuses;

[TestFixture]
public class when_sending_command_message_with_guid_already_set : BaseDatabaseTest
{
    private IBus _bus = null!;
    private CoreBus _coreBus = null!;
    private CreateScraperCommand _command = null!;

    [SetUp]
    public void Context()
    {
        _bus = A.Fake<IBus>();
        _coreBus = new CoreBus(
            _bus, 
            new ExistingTransactionJobCreator(UnitOfWork),
            A.Fake<IActingUserAccessor>(),
            A.Fake<IRequestIdAccessor>(),
            A.Fake<IRebusQueueRedirector>()
        );
        _command = new CreateScraperCommand(UserId: 23, "scraper name")
        {
            Guid = Guid.NewGuid()
        };
    }

    [Test]
    public void exception_is_thrown()
    {
        var ex = Should.Throw<Exception>(async () => await _coreBus.Send(_command));
        
        ex.Message.ShouldBe("Command message Guid is already set.");
    }
}