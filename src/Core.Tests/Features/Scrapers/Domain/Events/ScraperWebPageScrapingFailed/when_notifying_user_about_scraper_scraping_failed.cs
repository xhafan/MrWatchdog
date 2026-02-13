using FakeItEasy;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperWebPageScrapingFailed;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain.Events.ScraperWebPageScrapingFailed;

[TestFixture]
public class when_notifying_user_about_scraper_scraping_failed : BaseDatabaseTest
{
    private Scraper _scraper = null!;
    private long _scraperWebPageId;
    private ICoreBus _bus = null!;
    private User _user = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();

        _bus = A.Fake<ICoreBus>();
        
        var handler = new NotifyUserAboutScraperScrapingFailedDomainEventMessageHandler(
            new NhibernateRepository<Scraper>(UnitOfWork),
            _bus,
            OptionsTestRetriever.Retrieve<RuntimeOptions>()
        );

        await handler.Handle(new ScraperWebPageScrapingFailedDomainEvent(_scraper.Id));
    }

    [Test]
    public void email_notification_about_scraping_errors_is_sent_to_user()
    {
        A.CallTo(() => _bus.Send(A<SendEmailCommand>.That.Matches(p => _MatchingCommand(p)))).MustHaveHappenedOnceExactly();
    }

    private bool _MatchingCommand(SendEmailCommand command)
    {
        command.RecipientEmail.ShouldBe(_user.Email);
        command.Subject.ShouldContain("web scraping failed");
        command.Subject.ShouldContain("Epic Games store free game");
        command.Subject.ShouldNotContain(
            """
            { "en": "Epic Games store free game" }
            """
        );
        command.HtmlMessage.ShouldContain("Web scraping failed");
        command.HtmlMessage.ShouldContain(
            $"""
             <a href="https://mrwatchdog_test/Scrapers/Detail/{_scraper.Id}">Epic Games store free game</a>
             """
        );
        command.HtmlMessage.ShouldNotContain(
            """
            { "en": "Epic Games store free game" }
            """
        );        
        command.HtmlMessage.ShouldContain(
            $"""
             <a href="https://mrwatchdog_test/Scrapers/Detail/{_scraper.Id}#scraper_web_page_{_scraperWebPageId}">epic games store free games list</a>
             """
        );
        command.HtmlMessage.ShouldContain(
            """
            Network error
            """
        );
        return true;
    }
    
    private void _BuildEntities()
    {
        _user = new UserBuilder(UnitOfWork).Build();
        
        _scraper = new ScraperBuilder(UnitOfWork)
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "https://www.pcgamer.com/epic-games-store-free-games-list/",
                Selector = """
                           div#article-body p.infoUpdate-log a[href^="https://store.epicgames.com/"]
                           """,
                Name = """
                       { "en": "epic games store free games list" }
                       """
            })
            .WithName("""
                      { "en": "Epic Games store free game" }
                      """)
            .WithUser(_user)
            .Build();
        _scraperWebPageId = _scraper.WebPages.Single().Id;
        _scraper.SetScrapingErrorMessage(_scraperWebPageId, "Network error");
    }
}