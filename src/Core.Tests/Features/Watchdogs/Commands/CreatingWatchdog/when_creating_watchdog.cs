using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Commands.CreatingWatchdog;

[TestFixture]
public class when_creating_watchdog : BaseDatabaseTest
{
    private Scraper _scraper = null!;
    private Watchdog? _watchdog;
    private User _user = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        var handler = new CreateWatchdogCommandMessageHandler(
            new NhibernateRepository<Scraper>(UnitOfWork),
            new NhibernateRepository<Watchdog>(UnitOfWork),
            new UserRepository(UnitOfWork),
            UnitOfWork
        );

        await handler.Handle(new CreateWatchdogCommand(_scraper.Id, SearchTerm: "text") { ActingUserId = _user.Id});
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _watchdog = UnitOfWork.Session!.Query<Watchdog>()
            .SingleOrDefault(x => x.Scraper == _scraper);
    }

    [Test]
    public void new_watchdog_search_is_created_with_matching_current_scraping_results()
    {
        _watchdog.ShouldNotBeNull();
        _watchdog.User.ShouldBe(_user);
        _watchdog.ReceiveNotification.ShouldBe(true);
        _watchdog.SearchTerm.ShouldBe("text");
        _watchdog.CurrentScrapingResults.ShouldBe(["<div>tÉxt 1</div>", "<div>texŤ 3</div>"]);
    }
    
    private void _BuildEntities()
    {
        _user = new UserBuilder(UnitOfWork).Build();

        _scraper = new ScraperBuilder(UnitOfWork)
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .Build();
        var scraperWebPage = _scraper.WebPages.Single();
        _scraper.SetScrapingResults(scraperWebPage.Id, ["<div>tÉxt 1</div>", "<div>string 2</div>", "<div>texŤ 3</div>"]);
        _scraper.EnableWebPage(scraperWebPage.Id);
    }
}