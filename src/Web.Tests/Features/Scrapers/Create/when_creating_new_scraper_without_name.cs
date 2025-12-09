using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Web.Features.Scrapers.Create;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Create;

[TestFixture]
public class when_creating_new_scraper_without_name
{
    private IActionResult _actionResult = null!;
    private CreateModel _model = null!;

    [SetUp]
    public async Task Context()
    {
        _model = new CreateModelBuilder()
            .WithName("")
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
        var nameErrors = _model.ModelState[nameof(CreateModel.Name)]?.Errors;
        nameErrors.ShouldNotBeNull();
        nameErrors.ShouldContain(x => x.ErrorMessage.Contains("required"));
    }
}