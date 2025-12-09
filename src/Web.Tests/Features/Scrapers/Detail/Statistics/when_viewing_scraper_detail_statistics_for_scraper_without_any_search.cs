using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.Detail.Statistics;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Detail.Statistics;

[TestFixture]
public class when_viewing_scraper_detail_statistics_for_scraper_without_any_search : BaseDatabaseTest
{
    private StatisticsModel _model = null!;
    private Scraper _scraper = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new StatisticsModelBuilder(UnitOfWork)
            .Build();
        
        await _model.OnGet(_scraper.Id);
    }

    [Test]
    public void number_of_watchdog_searches_is_correct()
    {
        _model.PublicScraperStatistics.NumberOfUsersWithWatchdogWithNotification.ShouldBe(0);
        _model.PublicScraperStatistics.NumberOfUsersWithWatchdogWithoutNotification.ShouldBe(0);
    }

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork).Build();
    } 
}