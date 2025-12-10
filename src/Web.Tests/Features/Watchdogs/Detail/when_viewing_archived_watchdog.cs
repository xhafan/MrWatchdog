using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.Search;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Search;

[TestFixture]
public class when_viewing_archived_watchdog_search : BaseDatabaseTest
{
    private SearchModel _model = null!;
    private WatchdogSearch _watchdogSearch = null!;
    private Scraper _scraper = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new SearchModelBuilder(UnitOfWork).Build();
        
        await _model.OnGet(_watchdogSearch.Id);
    }

    [Test]
    public void model_is_correct()
    {
        _model.WatchdogSearchArgs.IsArchived.ShouldBe(true);
    }

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork).Build();
        
        _watchdogSearch = new WatchdogSearchBuilder(UnitOfWork)
            .WithScraper(_scraper)
            .Build();
        _watchdogSearch.Archive();

        UnitOfWork.Flush();
    }
}