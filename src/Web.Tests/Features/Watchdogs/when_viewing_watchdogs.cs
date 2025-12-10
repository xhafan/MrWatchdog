using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs;

namespace MrWatchdog.Web.Tests.Features.Watchdogs;

[TestFixture]
public class when_viewing_watchdogs : BaseDatabaseTest
{
    private Watchdog _watchdogForUserOne = null!;
    private IndexModel _model = null!;
    private User _userOne = null!;
    private User _userTwo = null!;
    private Watchdog _watchdogForUserTwo = null!;
    private Watchdog _archivedWatchdogForUserOne = null!;
    private Watchdog _watchdogForArchivedScraperForUserOne = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new IndexModelBuilder(UnitOfWork)
            .WithActingUser(_userOne)
            .Build();
        
        await _model.OnGet();
    }

    [Test]
    public void model_is_correct()
    {
        _model.Watchdogs.ShouldContain(
            new GetWatchdogsQueryResult
            {
                WatchdogId = _watchdogForUserOne.Id,
                ScraperName = "scraper name",
                SearchTerm = "search term",
                ReceiveNotification = true
            }
        );
        var watchdogIds = _model.Watchdogs.Select(x => x.WatchdogId).ToList();
        watchdogIds.ShouldNotContain(_watchdogForUserTwo.Id);
        watchdogIds.ShouldNotContain(_archivedWatchdogForUserOne.Id);
        watchdogIds.ShouldNotContain(_watchdogForArchivedScraperForUserOne.Id);
    }    
    
    private void _BuildEntities()
    {
        _userOne = new UserBuilder(UnitOfWork).Build();
        _userTwo = new UserBuilder(UnitOfWork).Build();
        
        var scraper = new ScraperBuilder(UnitOfWork)
            .WithName("scraper name")
            .Build();

        _watchdogForUserOne = new WatchdogBuilder(UnitOfWork)
            .WithScraper(scraper)
            .WithSearchTerm("search term")
            .WithUser(_userOne)
            .WithReceiveNotification(true)
            .Build();

        _archivedWatchdogForUserOne = new WatchdogBuilder(UnitOfWork)
            .WithScraper(scraper)
            .WithUser(_userOne)
            .Build();
        _archivedWatchdogForUserOne.Archive();

        _watchdogForUserTwo = new WatchdogBuilder(UnitOfWork)
            .WithScraper(scraper)
            .WithUser(_userTwo)
            .Build();

        var archivedScraper = new ScraperBuilder(UnitOfWork).Build();
        archivedScraper.Archive();

        _watchdogForArchivedScraperForUserOne = new WatchdogBuilder(UnitOfWork)
            .WithScraper(archivedScraper)
            .WithUser(_userOne)
            .Build();
    }
}