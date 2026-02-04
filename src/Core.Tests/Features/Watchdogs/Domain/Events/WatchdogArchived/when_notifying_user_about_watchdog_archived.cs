using FakeItEasy;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogArchived;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.Events.WatchdogArchived;

[TestFixture]
public class when_notifying_user_about_watchdog_archived : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;
    private ICoreBus _bus = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();

        _bus = A.Fake<ICoreBus>();

        var handler = new NotifyUserAboutWatchdogArchivedDomainEventMessageHandler(
            new NhibernateRepository<Watchdog>(UnitOfWork),
            _bus
        );

        await handler.Handle(new WatchdogArchivedDomainEvent(_watchdog.Id));
    }

    [Test]
    public void email_notification_about_watchdog_archived_is_sent()
    {
        A.CallTo(() => _bus.Send(A<SendEmailCommand>.That.Matches(p => _MatchingCommand(p)))).MustHaveHappenedOnceExactly();
    }

    private bool _MatchingCommand(SendEmailCommand command)
    {
        command.RecipientEmail.ShouldBe(_watchdog.User.Email);
        command.Subject.ShouldContain("watchdog");
        command.Subject.ShouldContain("has been deleted");
        command.HtmlMessage.ShouldContain("has been deleted");
        return true;
    }
    
    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork).Build();
    }
}