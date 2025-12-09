using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.Searches;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Searches;

[TestFixture]
public class when_viewing_watchdog_searches : BaseDatabaseTest
{
    private WatchdogSearch _watchdogSearchForUserOne = null!;
    private SearchesModel _model = null!;
    private User _userOne = null!;
    private User _userTwo = null!;
    private WatchdogSearch _watchdogSearchForUserTwo = null!;
    private WatchdogSearch _archivedWatchdogSearchForUserOne = null!;
    private WatchdogSearch _watchdogSearchForArchivedScraperForUserOne = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new SearchesModelBuilder(UnitOfWork)
            .WithActingUser(_userOne)
            .Build();
        
        await _model.OnGet();
    }

    [Test]
    public void model_is_correct()
    {
        _model.WatchdogSearches.ShouldContain(
            new GetWatchdogSearchesQueryResult
            {
                WatchdogSearchId = _watchdogSearchForUserOne.Id,
                ScraperName = "scraper name",
                SearchTerm = "search term",
                ReceiveNotification = true
            }
        );
        var watchdogSearchIds = _model.WatchdogSearches.Select(x => x.WatchdogSearchId).ToList();
        watchdogSearchIds.ShouldNotContain(_watchdogSearchForUserTwo.Id);
        watchdogSearchIds.ShouldNotContain(_archivedWatchdogSearchForUserOne.Id);
        watchdogSearchIds.ShouldNotContain(_watchdogSearchForArchivedScraperForUserOne.Id);
    }    
    
    private void _BuildEntities()
    {
        _userOne = new UserBuilder(UnitOfWork).Build();
        _userTwo = new UserBuilder(UnitOfWork).Build();
        
        var scraper = new ScraperBuilder(UnitOfWork)
            .WithName("scraper name")
            .Build();

        _watchdogSearchForUserOne = new WatchdogSearchBuilder(UnitOfWork)
            .WithScraper(scraper)
            .WithSearchTerm("search term")
            .WithUser(_userOne)
            .WithReceiveNotification(true)
            .Build();

        _archivedWatchdogSearchForUserOne = new WatchdogSearchBuilder(UnitOfWork)
            .WithScraper(scraper)
            .WithUser(_userOne)
            .Build();
        _archivedWatchdogSearchForUserOne.Archive();

        _watchdogSearchForUserTwo = new WatchdogSearchBuilder(UnitOfWork)
            .WithScraper(scraper)
            .WithUser(_userTwo)
            .Build();

        var archivedScraper = new ScraperBuilder(UnitOfWork).Build();
        archivedScraper.Archive();

        _watchdogSearchForArchivedScraperForUserOne = new WatchdogSearchBuilder(UnitOfWork)
            .WithScraper(archivedScraper)
            .WithUser(_userOne)
            .Build();
    }
}