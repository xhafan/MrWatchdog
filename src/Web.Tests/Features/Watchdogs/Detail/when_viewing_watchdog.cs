using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Detail;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail;

[TestFixture]
public class when_viewing_watchdog : BaseDatabaseTest
{
    private DetailModel _model = null!;
    private Watchdog _watchdog = null!;
    private Scraper _scraper = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new DetailModelBuilder(UnitOfWork).Build();
        
        _actionResult = await _model.OnGet(_watchdog.Id);
    }

    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<PageResult>();
    }

    [Test]
    public void model_is_correct()
    {
        _model.WatchdogArgs.WatchdogId.ShouldBe(_watchdog.Id);
        _model.WatchdogArgs.ScraperId.ShouldBe(_scraper.Id);
        _model.WatchdogArgs.SearchTerm.ShouldBe("text");
        
        _model.ScraperScrapedResultsArgs.ScraperId.ShouldBe(_scraper.Id);
        _model.ScraperScrapedResultsArgs.ScraperName.ShouldBe("scraper name");
        
        var webPageArgs = _model.ScraperScrapedResultsArgs.WebPages.ShouldHaveSingleItem();
        webPageArgs.Name.ShouldBe("url.com/page");
        webPageArgs.ScrapedResults.ShouldBe(["<div>text 1</div>"]);
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
        _scraper.SetScrapedResults(scraperWebPage.Id, ["<div>text 1</div>"]);
        _scraper.EnableWebPage(scraperWebPage.Id);
        
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithScraper(_scraper)
            .WithSearchTerm("text")
            .Build();

        UnitOfWork.Flush();
    }
}