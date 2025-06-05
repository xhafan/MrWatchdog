using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Detail.WebPageToMonitor;
using Rebus.Bus;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.WebPageToMonitor;

[TestFixture]
public class when_creating_watchdog_web_page_with_invalid_model_state : BaseDatabaseTest
{
    private IActionResult _actionResult = null!;
    private WebPageToMonitorModel _model = null!;
    private IBus _bus = null!;    
    private Watchdog _watchdog = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        _bus = A.Fake<IBus>();
        
        _model = new WebPageToMonitorModelBuilder(UnitOfWork)
            .WithBus(_bus)
            .WithWatchdogWebPageArgs(new WatchdogWebPageArgs
            {
                WatchdogId = _watchdog.Id,
                WatchdogWebPageId = 0,
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
        _watchdog = new WatchdogBuilder(UnitOfWork).Build();
    }    
}