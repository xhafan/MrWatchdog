using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.Detail.WebPage;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Detail.WebPage;

[TestFixture]
public class when_viewing_scraper_web_page_overview : BaseDatabaseTest
{
    private WebPageOverviewModel _model = null!;
    private Scraper _scraper = null!;
    private long _scraperWebPageId;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _model = new WebPageOverviewModelBuilder(UnitOfWork)
            .WithScraperWebPageArgs(new ScraperWebPageArgs
            {
                ScraperId = _scraper.Id,
                ScraperWebPageId = _scraperWebPageId
            })
            .Build();

        _actionResult = await _model.OnGet(_scraper.Id, scraperWebPageId: _scraperWebPageId);
    }

    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<PageResult>();
    }

    [Test]
    public void model_is_correct()
    {
        _model.ScraperWebPageArgs.ScraperId.ShouldBe(_scraper.Id);
        _model.ScraperWebPageArgs.ScraperWebPageId.ShouldBe(_scraperWebPageId);
        _model.ScraperWebPageArgs.Url.ShouldBe("http://url.com/page");
        _model.ScraperWebPageArgs.Selector.ShouldBe(".selector");
        _model.ScraperWebPageArgs.ScrapeHtmlAsRenderedByBrowser.ShouldBe(true);
        _model.ScraperWebPageArgs.SelectText.ShouldBe(true);
        _model.ScraperWebPageArgs.Name.ShouldBe("url.com/page");
        _model.ScraperWebPageArgs.HttpHeaders.ShouldBe("""
                                                        User-Agent: Mozilla/5.0
                                                        Connection: keep-alive
                                                        """, ignoreLineEndings: true);
        
        _model.IsEmptyWebPage.ShouldBe(false);
    }  

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork)
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                ScrapeHtmlAsRenderedByBrowser = true,
                SelectText = true,
                Name = "url.com/page",
                HttpHeaders = """
                              User-Agent: Mozilla/5.0
                              Connection: keep-alive
                              """
            })
            .Build();
        _scraperWebPageId = _scraper.WebPages.Single().Id;
    }    
}