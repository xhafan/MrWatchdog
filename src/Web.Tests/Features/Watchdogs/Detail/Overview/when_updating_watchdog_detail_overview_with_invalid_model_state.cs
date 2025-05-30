using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Detail.Overview;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.Overview;

[TestFixture]
public class when_updating_watchdog_detail_overview_with_invalid_model_state : BaseDatabaseTest
{
    private IActionResult _actionResult = null!;
    private OverviewModel _model = null!;
    private Watchdog _watchdog = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new OverviewModelBuilder(UnitOfWork)
            .Build();
        _model.WatchdogOverviewArgs = new WatchdogOverviewArgs
        {
            Id = _watchdog.Id,
            Name = null!
        };
        ModelValidator.ValidateModel(_model);

        _actionResult = await _model.OnPost(_watchdog.Id);
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
        const string nameKey = $"{nameof(WatchdogOverviewArgs)}.{nameof(WatchdogOverviewArgs.Name)}";
        _model.ModelState.Keys.ShouldBe([nameKey]);
        var nameErrors = _model.ModelState[nameKey]?.Errors;
        nameErrors.ShouldNotBeNull();
        nameErrors.ShouldContain(x => x.ErrorMessage.Contains("required"));
    }   

    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork).Build();
    }    
}