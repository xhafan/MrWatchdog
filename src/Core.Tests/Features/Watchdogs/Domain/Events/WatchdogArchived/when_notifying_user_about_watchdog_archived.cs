using FakeItEasy;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.WatchdogSearchArchived;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain.Events.WatchdogSearchArchived;

[TestFixture]
public class when_notifying_user_about_watchdog_search_archived : BaseDatabaseTest
{
    private WatchdogSearch _watchdogSearch = null!;
    private IEmailSender _emailSender = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();

        _emailSender = A.Fake<IEmailSender>();

        var handler = new NotifyUserAboutWatchdogSearchArchivedDomainEventMessageHandler(
            new NhibernateRepository<WatchdogSearch>(UnitOfWork),
            _emailSender
        );

        await handler.Handle(new WatchdogSearchArchivedDomainEvent(_watchdogSearch.Id));
    }

    [Test]
    public void email_notification_about_watchdog_search_archived_is_sent()
    {
        A.CallTo(() => _emailSender.SendEmail(
                _watchdogSearch.User.Email,
                A<string>.That.Matches(p => p.Contains("watchdog") && p.Contains("has been deleted")),
                A<string>.That.Matches(p => p.Contains("has been deleted")
                )
            ))
            .MustHaveHappenedOnceExactly();
    }
    
    private void _BuildEntities()
    {
        _watchdogSearch = new WatchdogSearchBuilder(UnitOfWork).Build();
    }
}