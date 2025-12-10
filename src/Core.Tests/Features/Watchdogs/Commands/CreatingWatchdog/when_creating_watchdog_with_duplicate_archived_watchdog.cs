using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Commands.CreatingWatchdogSearch;

[TestFixture]
public class when_creating_watchdog_search_with_duplicate_archived_watchdog_search : BaseDatabaseTest
{
    private Scraper _scraper = null!;
    private WatchdogSearch _archivedWatchdogSearch = null!;
    private User _user = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        var handler = new CreateWatchdogSearchCommandMessageHandler(
            new NhibernateRepository<Scraper>(UnitOfWork),
            new NhibernateRepository<WatchdogSearch>(UnitOfWork),
            new UserRepository(UnitOfWork),
            UnitOfWork
        );

        await handler.Handle(new CreateWatchdogSearchCommand(_scraper.Id, SearchTerm: "text") { ActingUserId = _user.Id});
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
    }

    [Test]
    public void new_watchdog_search_is_created()
    {
        UnitOfWork.Session!.Query<WatchdogSearch>().SingleOrDefault(x => x.Scraper == _scraper && !x.IsArchived).ShouldNotBeNull();
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
        _scraper.SetScrapingResults(scraperWebPage.Id, ["<div>text 1</div>", "<div>string 2</div>", "<div>text 3</div>"]);
        
        _archivedWatchdogSearch = new WatchdogSearchBuilder(UnitOfWork)
            .WithScraper(_scraper)
            .WithSearchTerm("text")
            .WithUser(_user)
            .Build();
        _archivedWatchdogSearch.Archive();
    }
}