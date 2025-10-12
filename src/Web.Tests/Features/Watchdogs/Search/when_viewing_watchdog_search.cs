using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Search;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Search;

[TestFixture]
public class when_viewing_watchdog_search : BaseDatabaseTest
{
    private SearchModel _model = null!;
    private WatchdogSearch _watchdogSearch = null!;
    private Watchdog _watchdog = null!;
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
        _model.WatchdogSearchArgs.WatchdogId.ShouldBe(_watchdog.Id);
        _model.WatchdogSearchArgs.SearchTerm.ShouldBe("text");
        
        _model.SearchTerm.ShouldBe("text");
        
        _model.WatchdogScrapingResultsArgs.WatchdogId.ShouldBe(_watchdog.Id);
        _model.WatchdogScrapingResultsArgs.WatchdogName.ShouldBe("watchdog name");
        
        var webPageArgs = _model.WatchdogScrapingResultsArgs.WebPages.ShouldHaveSingleItem();
        webPageArgs.Name.ShouldBe("url.com/page");
        webPageArgs.ScrapingResults.ShouldBe(["<div>text 1</div>"]);
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
        _watchdog.SetScrapingResults(watchdogWebPage.Id, ["<div>text 1</div>"]);
        _watchdog.EnableWebPage(watchdogWebPage.Id);
        
        _watchdogSearch = new WatchdogSearchBuilder(UnitOfWork)
            .WithWatchdog(_watchdog)
            .WithSearchTerm("text")
            .Build();

        UnitOfWork.Flush();
    }
}