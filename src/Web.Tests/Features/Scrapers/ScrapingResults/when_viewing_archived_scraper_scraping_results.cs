using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.ScrapingResults;

namespace MrWatchdog.Web.Tests.Features.Scrapers.ScrapingResults;

[TestFixture]
public class when_viewing_archived_scraper_scraping_results : BaseDatabaseTest
{
    private ScrapingResultsModel _model = null!;
    private Scraper _scraper = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new ScrapingResultsModelBuilder(UnitOfWork)
            .Build();
        
        await _model.OnGet(_scraper.Id);
    }

    [Test]
    public void model_is_correct()
    {
        _model.ScraperScrapingResultsArgs.IsArchived.ShouldBe(true);
    }

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork).Build();
        _scraper.Archive();
    }    
}