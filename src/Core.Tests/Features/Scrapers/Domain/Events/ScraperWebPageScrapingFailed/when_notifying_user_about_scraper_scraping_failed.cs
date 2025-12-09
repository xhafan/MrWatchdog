using FakeItEasy;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperWebPageScrapingFailed;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain.Events.ScraperWebPageScrapingFailed;

[TestFixture]
public class when_notifying_user_about_scraper_scraping_failed : BaseDatabaseTest
{
    private Scraper _scraper = null!;
    private long _scraperWebPageId;
    private IEmailSender _emailSender = null!;
    private User _user = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();

        _emailSender = A.Fake<IEmailSender>();
        
        var handler = new NotifyUserAboutScraperScrapingFailedDomainEventMessageHandler(
            new NhibernateRepository<Scraper>(UnitOfWork),
            _emailSender,
            OptionsTestRetriever.Retrieve<RuntimeOptions>()
        );

        await handler.Handle(new ScraperWebPageScrapingFailedDomainEvent(_scraper.Id));
    }

    [Test]
    public void email_notification_about_scraping_errors_is_sent_to_user()
    {
        A.CallTo(() => _emailSender.SendEmail(
                _user.Email,
                A<string>.That.Matches(p => p.Contains("web scraping failed") && p.Contains("Epic Games store free game")),
                A<string>.That.Matches(p => p.Contains("Web scraping failed")
                                            && p.Contains($"""
                                                           <a href="https://mrwatchdog_test/Scrapers/Detail/{_scraper.Id}">Epic Games store free game</a>
                                                           """)
                                            && p.Contains($"""
                                                           <a href="https://mrwatchdog_test/Scrapers/Detail/{_scraper.Id}#scraper_web_page_{_scraperWebPageId}">www.pcgamer.com/epic-games-store-free-games-list/</a>
                                                           """)
                                            && p.Contains("""
                                                           Network error
                                                           """)
                )
            ))
            .MustHaveHappenedOnceExactly();
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
                Name = "www.pcgamer.com/epic-games-store-free-games-list/"
            })
            .WithName("Epic Games store free game")
            .WithUser(_user)
            .Build();
        _scraperWebPageId = _scraper.WebPages.Single().Id;
        _scraper.SetScrapingErrorMessage(_scraperWebPageId, "Network error");
    }
}