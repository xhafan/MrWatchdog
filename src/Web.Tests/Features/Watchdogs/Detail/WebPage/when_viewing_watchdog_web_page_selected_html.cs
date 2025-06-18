using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.WebPage;

[TestFixture]
public class when_viewing_watchdog_web_page_selected_html : BaseDatabaseTest
{
    private WebPageSelectedHtmlModel _model = null!;
    private Watchdog _watchdog = null!;
    private long _watchdogWebPageId;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _model = new WebPageSelectedHtmlModelBuilder(UnitOfWork).Build();

        await _model.OnGet(_watchdog.Id, watchdogWebPageId: _watchdogWebPageId);
    }

    [Test]
    public void model_is_correct()
    {
        _model.WatchdogWebPageSelectedHtmlDto.WatchdogId.ShouldBe(_watchdog.Id);
        _model.WatchdogWebPageSelectedHtmlDto.WatchdogWebPageId.ShouldBe(_watchdogWebPageId);
        _model.WatchdogWebPageSelectedHtmlDto.SelectedHtml.ShouldBe("<div></div>");
        _model.WatchdogWebPageSelectedHtmlDto.ScrapedOn.ShouldNotBeNull();
        _model.WatchdogWebPageSelectedHtmlDto.ScrapedOn.Value.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
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
        _watchdog.SetSelectedHtml(_watchdogWebPageId, "<div></div>");
    }    
}