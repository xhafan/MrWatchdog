using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;
using MrWatchdog.Web.Features.Watchdogs.Alert;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Alert;

[TestFixture]
public class when_viewing_watchdog_alert : BaseDatabaseTest
{
    private AlertModel _model = null!;
    private WatchdogAlert _watchdogAlert = null!;
    private Watchdog _watchdog = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new AlertModelBuilder(UnitOfWork).Build();
        
        await _model.OnGet(_watchdogAlert.Id);
    }

    [Test]
    public void model_is_correct()
    {
        _model.WatchdogAlertArgs.WatchdogAlertId.ShouldBe(_watchdogAlert.Id);
        _model.WatchdogAlertArgs.WatchdogId.ShouldBe(_watchdog.Id);
        _model.WatchdogAlertArgs.SearchTerm.ShouldBe("text");
        
        _model.SearchTerm.ShouldBe("text");
        
        _model.WatchdogScrapingResultsArgs.WatchdogId.ShouldBe(_watchdog.Id);
        _model.WatchdogScrapingResultsArgs.WatchdogName.ShouldBe("watchdog name");
        
        var webPageArgs = _model.WatchdogScrapingResultsArgs.WebPages.ShouldHaveSingleItem();
        webPageArgs.Name.ShouldBe("url.com/page");
        webPageArgs.SelectedElements.ShouldBe(["<div>text 1</div>"]);
        webPageArgs.Url.ShouldBe("http://url.com/page");        
    }

    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithName("watchdog name")
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .Build();
        var watchdogWebPage = _watchdog.WebPages.Single();
        _watchdog.SetSelectedElements(watchdogWebPage.Id, ["<div>text 1</div>"]);
        UnitOfWork.Save(_watchdog);
        
        _watchdogAlert = new WatchdogAlertBuilder(UnitOfWork)
            .WithWatchdog(_watchdog)
            .WithSearchTerm("text")
            .Build();
    }
}