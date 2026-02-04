using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Commands.CreatingWatchdog;

[TestFixture]
public class when_creating_duplicate_watchdog : BaseDatabaseTest
{
    private Scraper _scraper = null!;
    private Watchdog _watchdog = null!;
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
    }

    [Test]
    public void new_watchdog_is_not_created()
    {
        UnitOfWork.Session!.Query<Watchdog>().Single(x => x.Scraper == _scraper).ShouldBe(_watchdog);
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
        _scraper.SetScrapedResults(scraperWebPage.Id, ["<div>text 1</div>", "<div>string 2</div>", "<div>text 3</div>"]);
        
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithScraper(_scraper)
            .WithSearchTerm("text")
            .WithUser(_user)
            .Build();
    }
}