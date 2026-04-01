using CoreBackend.Infrastructure.EmailSenders;
using CoreBackend.Infrastructure.Rebus;
using CoreBackend.TestsShared;
using CoreDdd.Nhibernate.Repositories;
using FakeItEasy;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogArchived;
using MrWatchdog.Core.Infrastructure.Localization;
using MrWatchdog.Core.TestsShared.Builders;

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
        command.Subject.ShouldContain("hlídací pes");
        command.Subject.ShouldContain("jméno scraperu");
        command.Subject.ShouldNotContain("""
                                         { "en": "scraper name", "cs": "jméno scraperu" }
                                         """);
        command.Subject.ShouldContain("byl smazán");
        command.HtmlMessage.ShouldContain("byl smazán");
        command.HtmlMessage.ShouldContain("jméno scraperu");
        command.HtmlMessage.ShouldNotContain("""
                                             { "en": "scraper name", "cs": "jméno scraperu" }
                                             """);
        return true;
    }
    
    private void _BuildEntities()
    {
        var scraper = new ScraperBuilder(UnitOfWork)
            .WithName("""
                      { "en": "scraper name", "cs": "jméno scraperu" }
                      """)
            .Build();

        var user = new UserBuilder(UnitOfWork)
            .WithCulture(CultureConstants.Cs)
            .Build();

        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithScraper(scraper)
            .WithUser(user)
            .Build();
    }
}