using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.WebPage;

[TestFixture]
public class when_viewing_watchdog_web_page_scraping_results : BaseDatabaseTest
{
    private WebPageScrapingResultsModel _model = null!;
    private Watchdog _watchdog = null!;
    private long _watchdogWebPageId;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _model = new WebPageScrapingResultsModelBuilder(UnitOfWork).Build();

        await _model.OnGet(_watchdog.Id, watchdogWebPageId: _watchdogWebPageId);
    }

    [Test]
    public void model_is_correct()
    {
        _model.WatchdogWebPageScrapingResultsDto.WatchdogId.ShouldBe(_watchdog.Id);
        _model.WatchdogWebPageScrapingResultsDto.WatchdogWebPageId.ShouldBe(_watchdogWebPageId);
        _model.WatchdogWebPageScrapingResultsDto.ScrapingResults.ShouldBe(["<div>text</div>"]);
        _model.WatchdogWebPageScrapingResultsDto.ScrapedOn.ShouldNotBeNull();
        _model.WatchdogWebPageScrapingResultsDto.ScrapedOn.Value.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
        _model.WatchdogWebPageScrapingResultsDto.ScrapingErrorMessage.ShouldBe(null);
    }  

    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .Build();
        _watchdogWebPageId = _watchdog.WebPages.Single().Id;
        _watchdog.SetScrapingResults(_watchdogWebPageId, ["<div>text</div>"]);
    }    
}