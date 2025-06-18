using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.WebPage;

[TestFixture]
public class when_updating_watchdog_web_page_with_invalid_model_state : BaseDatabaseTest
{
    private IActionResult _actionResult = null!;
    private WebPageOverviewModel _model = null!;
    private Watchdog _watchdog = null!;
    private long _watchdogWebPageId;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new WebPageOverviewModelBuilder(UnitOfWork)
            .WithWatchdogWebPageArgs(new WatchdogWebPageArgs
            {
                WatchdogId = _watchdog.Id,
                WatchdogWebPageId = _watchdogWebPageId,
                Url = "http",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .Build();

        _actionResult = await _model.OnPost();
    }

    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<PageResult>();
        var pageResult = (PageResult) _actionResult;
        pageResult.StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
    }

    [Test]
    public void model_is_invalid()
    {
        _model.ModelState.IsValid.ShouldBe(false);
        const string key = $"{nameof(WatchdogWebPageArgs)}.{nameof(WatchdogWebPageArgs.Url)}";
        _model.ModelState.Keys.ShouldBe([key]);
        var urlErrors = _model.ModelState[key]?.Errors;
        urlErrors.ShouldNotBeNull();
        urlErrors.ShouldContain(x => x.ErrorMessage.Contains("valid"));
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
    }    
}