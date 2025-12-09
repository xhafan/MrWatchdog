using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.Detail.Overview;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Detail.Overview;

[TestFixture]
public class when_viewing_scraper_detail_overview : BaseDatabaseTest
{
    private OverviewModel _model = null!;
    private Scraper _scraper = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new OverviewModelBuilder(UnitOfWork)
            .Build();
        
        _actionResult = await _model.OnGet(_scraper.Id);
    }

    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<PageResult>();
    }

    [Test]
    public void model_is_correct()
    {
        _model.ScraperOverviewArgs.ScraperId.ShouldBe(_scraper.Id);
        _model.ScraperOverviewArgs.Name.ShouldBe(ScraperBuilder.Name);
        _model.ScraperOverviewArgs.Description.ShouldBe(ScraperBuilder.Description);
        _model.ScraperOverviewArgs.ScrapingIntervalInSeconds.ShouldBe(ScraperBuilder.ScrapingIntervalInSeconds);
        _model.ScraperOverviewArgs.IntervalBetweenSameResultNotificationsInDays.ShouldBe(ScraperBuilder.IntervalBetweenSameResultNotificationsInDays);
        _model.ScraperOverviewArgs.NumberOfFailedScrapingAttemptsBeforeAlerting.ShouldBe(ScraperBuilder.NumberOfFailedScrapingAttemptsBeforeAlerting);
    }

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork).Build();
    }    
}