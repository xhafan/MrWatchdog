using FakeItEasy;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogArchived;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.Events.WatchdogArchived;

[TestFixture]
public class when_notifying_user_about_watchdog_archived : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;
    private IEmailSender _emailSender = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();

        _emailSender = A.Fake<IEmailSender>();

        var handler = new NotifyUserAboutWatchdogArchivedDomainEventMessageHandler(
            new NhibernateRepository<Watchdog>(UnitOfWork),
            _emailSender
        );

        await handler.Handle(new WatchdogArchivedDomainEvent(_watchdog.Id));
    }

    [Test]
    public void email_notification_about_watchdog_search_archived_is_sent()
    {
        A.CallTo(() => _emailSender.SendEmail(
                _watchdog.User.Email,
                A<string>.That.Matches(p => p.Contains("watchdog") && p.Contains("has been deleted")),
                A<string>.That.Matches(p => p.Contains("has been deleted")
                )
            ))
            .MustHaveHappenedOnceExactly();
    }
    
    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork).Build();
    }
}