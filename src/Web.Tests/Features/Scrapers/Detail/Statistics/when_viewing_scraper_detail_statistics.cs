using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.Detail.Statistics;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Detail.Statistics;

[TestFixture]
public class when_viewing_scraper_detail_statistics : BaseDatabaseTest
{
    private StatisticsModel _model = null!;
    private Scraper _scraper = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new StatisticsModelBuilder(UnitOfWork)
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
        _model.ScraperId.ShouldBe(_scraper.Id);

        _model.PublicScraperStatistics.CalculatedEarningsForThisMonth.ShouldBe(0);
        _model.PublicScraperStatistics.NumberOfUsersWithWatchdogWithNotification.ShouldBe(2);
        _model.PublicScraperStatistics.NumberOfUsersWithWatchdogWithoutNotification.ShouldBe(1);

        _model.ScraperPublicStatus.ShouldBe(PublicStatus.Public);
    }

    private void _BuildEntities()
    {
        var scraperOwner = new UserBuilder(UnitOfWork).Build();

        _scraper = new ScraperBuilder(UnitOfWork)
            .WithUser(scraperOwner)
            .Build();
        _scraper.MakePublic();

        // ReSharper disable UnusedVariable
        var ownersWatchdogWithNotification = new WatchdogBuilder(UnitOfWork)
            .WithScraper(_scraper)
            .WithUser(scraperOwner)
            .WithReceiveNotification(true)
            .Build();

        var ownersWatchdogWithoutNotification = new WatchdogBuilder(UnitOfWork)
            .WithScraper(_scraper)
            .WithUser(scraperOwner)
            .WithReceiveNotification(false)
            .Build();

        var userOne = new UserBuilder(UnitOfWork).Build();
        var userTwo = new UserBuilder(UnitOfWork).Build();

        var watchdogOneWithNotification = new WatchdogBuilder(UnitOfWork)
            .WithScraper(_scraper)
            .WithUser(userOne)
            .WithReceiveNotification(true)
            .Build();

        var watchdogTwoWithNotification = new WatchdogBuilder(UnitOfWork)
            .WithScraper(_scraper)
            .WithUser(userOne)
            .WithReceiveNotification(true)
            .Build();

        var watchdogThreeWithoutNotification = new WatchdogBuilder(UnitOfWork)
            .WithScraper(_scraper)
            .WithUser(userTwo)
            .WithReceiveNotification(true)
            .Build();

        var watchdogFourWithoutNotification = new WatchdogBuilder(UnitOfWork)
            .WithScraper(_scraper)
            .WithUser(userTwo)
            .WithReceiveNotification(false)
            .Build();

        var watchdogFiveWithoutNotification = new WatchdogBuilder(UnitOfWork)
            .WithScraper(_scraper)
            .WithUser(userTwo)
            .WithReceiveNotification(false)
            .Build();

        var watchdogForAnotherScraper = new WatchdogBuilder(UnitOfWork)
            .Build();
        // ReSharper restore UnusedVariable
    }    
}