using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.ScrapingResults;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.ScrapingResults;

[TestFixture]
public class when_viewing_watchdog_scraping_results_for_public_watchdog_as_unauthenticated_user : BaseDatabaseTest
{
    private ScrapingResultsModel _model = null!;
    private Watchdog _watchdog = null!;
    private User _user = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new ScrapingResultsModelBuilder(UnitOfWork)
            .Build();
        
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
        _model.WatchdogScrapingResultsArgs.WatchdogId.ShouldBe(_watchdog.Id);
        _model.WatchdogScrapingResultsArgs.WatchdogName.ShouldBe("watchdog name");
        
        var webPageArgs = _model.WatchdogScrapingResultsArgs.WebPages.ShouldHaveSingleItem();
        webPageArgs.Name.ShouldBe("url.com/page");
        webPageArgs.ScrapingResults.ShouldBe(["<div>text 1</div>", "<div>text 2</div>"]);
        webPageArgs.Url.ShouldBe("http://url.com/page");
        
        _model.WatchdogScrapingResultsArgs.UserId.ShouldBe(_user.Id);
    }

    private void _BuildEntities()
    {
        _user = new UserBuilder(UnitOfWork).Build();
        
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithName("watchdog name")
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .WithUser(_user)
            .Build();
        var watchdogWebPage = _watchdog.WebPages.Single();
        _watchdog.SetScrapingResults(watchdogWebPage.Id, ["<div>text 1</div>", "<div>text 2</div>"]);
        _watchdog.EnableWebPage(watchdogWebPage.Id);
        _watchdog.MakePublic();
        
        UnitOfWork.Flush();
    }    
}