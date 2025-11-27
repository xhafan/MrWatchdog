using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.WebPage;

[TestFixture]
public class when_viewing_watchdog_web_page_overview : BaseDatabaseTest
{
    private WebPageOverviewModel _model = null!;
    private Watchdog _watchdog = null!;
    private long _watchdogWebPageId;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _model = new WebPageOverviewModelBuilder(UnitOfWork)
            .WithWatchdogWebPageArgs(new WatchdogWebPageArgs
            {
                WatchdogId = _watchdog.Id,
                WatchdogWebPageId = _watchdogWebPageId
            })
            .Build();

        _actionResult = await _model.OnGet(_watchdog.Id, watchdogWebPageId: _watchdogWebPageId);
    }

    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<PageResult>();
    }

    [Test]
    public void model_is_correct()
    {
        _model.WatchdogWebPageArgs.WatchdogId.ShouldBe(_watchdog.Id);
        _model.WatchdogWebPageArgs.WatchdogWebPageId.ShouldBe(_watchdogWebPageId);
        _model.WatchdogWebPageArgs.Url.ShouldBe("http://url.com/page");
        _model.WatchdogWebPageArgs.Selector.ShouldBe(".selector");
        _model.WatchdogWebPageArgs.SelectText.ShouldBe(true);
        _model.WatchdogWebPageArgs.Name.ShouldBe("url.com/page");
        _model.WatchdogWebPageArgs.HttpHeaders.ShouldBe("""
                                                        User-Agent: Mozilla/5.0
                                                        Connection: keep-alive
                                                        """, ignoreLineEndings: true);
        
        _model.IsEmptyWebPage.ShouldBe(false);
    }  

    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                SelectText = true,
                Name = "url.com/page",
                HttpHeaders = """
                              User-Agent: Mozilla/5.0
                              Connection: keep-alive
                              """
            })
            .Build();
        _watchdogWebPageId = _watchdog.WebPages.Single().Id;
    }    
}