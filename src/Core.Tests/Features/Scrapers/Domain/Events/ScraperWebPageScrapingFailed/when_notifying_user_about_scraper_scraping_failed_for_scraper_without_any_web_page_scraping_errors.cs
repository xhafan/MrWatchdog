using FakeItEasy;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperWebPageScrapingFailed;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.Core.Messages;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain.Events.ScraperWebPageScrapingFailed;

[TestFixture]
public class when_notifying_user_about_scraper_scraping_failed_for_scraper_without_any_web_page_scraping_errors : BaseDatabaseTest
{
    private Scraper _scraper = null!;
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
    public void email_notification_about_scraping_errors_is_not_sent_to_user()
    {
        A.CallTo(() => _bus.Send(A<Command>._)).MustNotHaveHappened();
    }
    
    private void _BuildEntities()
    {
        _user = new UserBuilder(UnitOfWork).Build();
        
        _scraper = new ScraperBuilder(UnitOfWork)
            .WithUser(_user)
            .Build();
    }
}