using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.ScrapedResults;

namespace MrWatchdog.Web.Tests.Features.Scrapers.ScrapedResults;

[TestFixture]
public class when_viewing_archived_scraper_scraped_results : BaseDatabaseTest
{
    private ScrapedResultsModel _model = null!;
    private Scraper _scraper = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new ScrapedResultsModelBuilder(UnitOfWork)
            .Build();
        
        await _model.OnGet(_scraper.Id);
    }

    [Test]
    public void model_is_correct()
    {
        _model.ScraperScrapedResultsArgs.IsArchived.ShouldBe(true);
    }

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork).Build();
        _scraper.Archive();
    }    
}