using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.ScrapingResults;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.ScrapingResults;

[TestFixture]
public class when_creating_watchdog_search_with_too_long_search_term : BaseDatabaseTest
{
    private ScrapingResultsModel _model = null!;
    private Watchdog _watchdog = null!;
    private IActionResult _actionResult = null!;
    private User _actingUser = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new ScrapingResultsModelBuilder(UnitOfWork)
            .WithActingUser(_actingUser)
            .WithSearchTerm(new string('x', ValidationConstants.SearchTermMaxLength + 1))
            .Build();

        _actionResult = await _model.OnPostCreateWatchdogSearch(_watchdog.Id);
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
        var errors = _model.ModelState[$"{nameof(ScrapingResultsModel.SearchTerm)}"]?.Errors;
        errors.ShouldNotBeNull();
        errors.Select(x => x.ErrorMessage).ShouldBe(["The field SearchTerm must be a string with a maximum length of 400."]);
    }

    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork).Build();

        _actingUser = new UserBuilder(UnitOfWork).Build();
    }    
}