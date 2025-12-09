using FakeItEasy;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Infrastructure.ActingUserAccessors;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Rebus.RebusQueueRedirectors;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.Core.Infrastructure.RequestIdAccessors;
using MrWatchdog.TestsShared;
using Rebus.Bus;
using Rebus.Messages;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.CoreBuses;

[TestFixture]
public class when_sending_command_message : BaseDatabaseTest
{
    private IBus _bus = null!;
    private CoreBus _coreBus = null!;
    private CreateScraperCommand _command = null!;

    [SetUp]
    public async Task Context()
    {
        _bus = A.Fake<IBus>();
        
        var actingUserAccessor = A.Fake<IActingUserAccessor>();
        A.CallTo(() => actingUserAccessor.GetActingUserId()).Returns(23);

        var requestIdAccessor = A.Fake<IRequestIdAccessor>();
        A.CallTo(() => requestIdAccessor.GetRequestId()).Returns("0HNFBP8T98MQS:00000045");

        var rebusQueueRedirector = A.Fake<IRebusQueueRedirector>();
        A.CallTo(() => rebusQueueRedirector.GetQueueForRedirection()).Returns(null);

        _coreBus = new CoreBus(
            _bus,
            new ExistingTransactionJobCreator(UnitOfWork),
            actingUserAccessor,
            requestIdAccessor,
            rebusQueueRedirector
        );
        _command = new CreateScraperCommand(UserId: 23, "scraper name");
        
        await _coreBus.Send(_command);
    }

    [Test]
    public void command_message_is_sent_over_the_bus()
    {
        A.CallTo(() => _bus.Send(
                A<CreateScraperCommand>.That.Matches(p => p.Name == "scraper name" 
                                                           && !p.Guid.Equals(Guid.Empty)
                                                           && p.ActingUserId == 23
                                                           && p.RequestId == "0HNFBP8T98MQS:00000045"
                                                           ),
                A<IDictionary<string, string>>.That.Matches(p => p.ContainsKey(Headers.MessageId) 
                                                                 && p[Headers.MessageId] == _command.Guid.ToString()
                                                                 
                                                                 && !p.ContainsKey(CustomHeaders.QueueForRedirection))
            )
        ).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task command_job_is_created()
    {
        var job = await new JobRepository(UnitOfWork).GetByGuidAsync(_command.Guid);
        job.ShouldNotBeNull();
        job.Guid.ShouldBe(_command.Guid);
        job.Type.ShouldBe(nameof(CreateScraperCommand));
        job.HandlingAttempts.ShouldBeEmpty();
        job.RequestId.ShouldBe("0HNFBP8T98MQS:00000045");
    }
}