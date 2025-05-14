using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Web.Pages.Watchdogs;

namespace MrWatchdog.Web.Tests.Pages.Watchdogs.Detail;

[TestFixture]
public class when_creating_new_watchdog_without_name
{
    private IActionResult _actionResult = null!;
    private CreateModel _createModel = null!;

    [SetUp]
    public async Task Context()
    {
        _createModel = new CreateModelBuilder()
            .WithName("")
            .Build();

        _actionResult = await _createModel.OnPost();
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
        _createModel.ModelState.IsValid.ShouldBe(false);
        var nameErrors = _createModel.ModelState[nameof(CreateModel.Name)]?.Errors;
        nameErrors.ShouldNotBeNull();
        nameErrors.ShouldContain(x => x.ErrorMessage.Contains("required"));
    }
}