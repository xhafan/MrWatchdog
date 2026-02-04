using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.ScrapedResults;

namespace MrWatchdog.Web.Tests.Features.Scrapers.ScrapedResults;

[TestFixture]
public class when_viewing_scraper_scraped_results_with_empty_scraper_web_page_name : BaseDatabaseTest
{
    private ScrapedResultsModel _model = null!;
    private Scraper _scraper = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new ScrapedResultsModelBuilder(UnitOfWork)
            .Build();
        
        _actionResult = await _model.OnGet(_scraper.Id);
    }

    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<PageResult>();
    }

    [Test]
    public void scraper_scraping_result_web_pages_are_empty()
    {
        _model.ScraperScrapedResultsArgs.WebPages.ShouldBeEmpty();
    }

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork)
            .WithName("scraper name")
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = null
            })
            .Build();
        _scraper.MakePublic();

        UnitOfWork.Flush();
    }    
}