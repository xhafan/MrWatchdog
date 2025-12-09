using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.Detail.Overview;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Detail.Overview;

[TestFixture]
public class when_updating_scraper_detail_overview_with_invalid_model_state : BaseDatabaseTest
{
    private IActionResult _actionResult = null!;
    private OverviewModel _model = null!;
    private Scraper _scraper = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new OverviewModelBuilder(UnitOfWork)
            .Build();
        _model.ScraperOverviewArgs = new ScraperOverviewArgs
        {
            ScraperId = _scraper.Id,
            Name = null!,
            Description = null,
            ScrapingIntervalInSeconds = 0,
            IntervalBetweenSameResultNotificationsInDays = 30,
            NumberOfFailedScrapingAttemptsBeforeAlerting = 5
        };
        ModelValidator.ValidateModel(_model);

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
        
        var key = $"{nameof(ScraperOverviewArgs)}.{nameof(ScraperOverviewArgs.Name)}";
        _model.ModelState.ShouldContainKey(key);
        var nameErrors = _model.ModelState[key]?.Errors;
        nameErrors.ShouldNotBeNull();
        nameErrors.ShouldContain(x => x.ErrorMessage.Contains("required"));
        
        key = $"{nameof(ScraperOverviewArgs)}.{nameof(ScraperOverviewArgs.ScrapingIntervalInSeconds)}";
        _model.ModelState.ShouldContainKey(key);
        var scrapingIntervalInSecondsErrors = _model.ModelState[key]?.Errors;
        scrapingIntervalInSecondsErrors.ShouldNotBeNull();
        scrapingIntervalInSecondsErrors.ShouldContain(x => x.ErrorMessage.Contains("must be between"));
        
    }   

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork).Build();
    }    
}