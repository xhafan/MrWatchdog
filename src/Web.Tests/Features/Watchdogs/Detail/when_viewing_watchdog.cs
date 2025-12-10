using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.Search;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Search;

[TestFixture]
public class when_viewing_watchdog_search : BaseDatabaseTest
{
    private SearchModel _model = null!;
    private WatchdogSearch _watchdogSearch = null!;
    private Scraper _scraper = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new SearchModelBuilder(UnitOfWork).Build();
        
        _actionResult = await _model.OnGet(_watchdogSearch.Id);
    }

    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<PageResult>();
    }

    [Test]
    public void model_is_correct()
    {
        _model.WatchdogSearchArgs.WatchdogSearchId.ShouldBe(_watchdogSearch.Id);
        _model.WatchdogSearchArgs.ScraperId.ShouldBe(_scraper.Id);
        _model.WatchdogSearchArgs.SearchTerm.ShouldBe("text");
        
        _model.ScraperScrapingResultsArgs.ScraperId.ShouldBe(_scraper.Id);
        _model.ScraperScrapingResultsArgs.ScraperName.ShouldBe("scraper name");
        
        var webPageArgs = _model.ScraperScrapingResultsArgs.WebPages.ShouldHaveSingleItem();
        webPageArgs.Name.ShouldBe("url.com/page");
        webPageArgs.ScrapingResults.ShouldBe(["<div>text 1</div>"]);
        webPageArgs.Url.ShouldBe("http://url.com/page");        
    }

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork)
            .WithName("scraper name")
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .Build();
        var scraperWebPage = _scraper.WebPages.Single();
        _scraper.SetScrapingResults(scraperWebPage.Id, ["<div>text 1</div>"]);
        _scraper.EnableWebPage(scraperWebPage.Id);
        
        _watchdogSearch = new WatchdogSearchBuilder(UnitOfWork)
            .WithScraper(_scraper)
            .WithSearchTerm("text")
            .Build();

        UnitOfWork.Flush();
    }
}