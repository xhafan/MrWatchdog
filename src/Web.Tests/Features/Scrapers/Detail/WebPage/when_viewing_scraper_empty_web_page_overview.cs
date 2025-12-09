using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.Detail.WebPage;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Detail.WebPage;

[TestFixture]
public class when_viewing_scraper_empty_web_page_overview : BaseDatabaseTest
{
    private WebPageOverviewModel _model = null!;
    private Scraper _scraper = null!;
    private long _scraperWebPageId;

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
                ScraperWebPageId = _scraperWebPageId,
                Url = null,
                Selector = null,
                Name = null
            })
            .Build();

        await _model.OnGet(_scraper.Id, scraperWebPageId: _scraperWebPageId);
    }

    [Test]
    public void model_is_correct()
    {
        _model.IsEmptyWebPage.ShouldBe(true);
    }  

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork)
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = null,
                Selector = null,
                Name = null
            })
            .Build();
        _scraperWebPageId = _scraper.WebPages.Single().Id;
    }    
}