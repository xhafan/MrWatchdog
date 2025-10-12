using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.WebPage;

[TestFixture]
public class when_viewing_watchdog_web_page_disabled_warning : BaseDatabaseTest
{
    private WebPageDisabledWarningModel _model = null!;
    private Watchdog _watchdog = null!;
    private long _watchdogWebPageId;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _model = new WebPageDisabledWarningModelBuilder(UnitOfWork)
            .WithWatchdogId(_watchdog.Id)
            .WithWatchdogWebPageId(_watchdogWebPageId)
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
        _model.WatchdogWebPageDisabledWarningDto.IsEnabled.ShouldBe(true);
        _model.WatchdogWebPageDisabledWarningDto.HasBeenScrapedSuccessfully.ShouldBe(true);
    }  
    
    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                SelectText = true,
                Name = "url.com/page"
            })
            .Build();
        _watchdogWebPageId = _watchdog.WebPages.Single().Id;
        _watchdog.SetScrapingResults(_watchdogWebPageId, ["Another World", "Doom 1"]);
        _watchdog.EnableWebPage(_watchdogWebPageId);
    }    
}