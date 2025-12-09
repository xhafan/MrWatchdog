using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.Detail.WebPage;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Detail.WebPage;

[TestFixture]
public class when_viewing_scraper_web_page_disabled_warning : BaseDatabaseTest
{
    private WebPageDisabledWarningModel _model = null!;
    private Scraper _scraper = null!;
    private long _scraperWebPageId;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _model = new WebPageDisabledWarningModelBuilder(UnitOfWork)
            .WithScraperId(_scraper.Id)
            .WithScraperWebPageId(_scraperWebPageId)
            .Build();

        _actionResult = await _model.OnGet();
    }

    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<PageResult>();
    }

    [Test]
    public void model_is_correct()
    {
        _model.ScraperWebPageDisabledWarningDto.IsEnabled.ShouldBe(true);
        _model.ScraperWebPageDisabledWarningDto.HasBeenScrapedSuccessfully.ShouldBe(true);
    }  
    
    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork)
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                SelectText = true,
                Name = "url.com/page"
            })
            .Build();
        _scraperWebPageId = _scraper.WebPages.Single().Id;
        _scraper.SetScrapingResults(_scraperWebPageId, ["Another World", "Doom 1"]);
        _scraper.EnableWebPage(_scraperWebPageId);
    }    
}