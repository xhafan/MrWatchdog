using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.Detail.Badges;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Detail.Badges;

[TestFixture]
public class when_viewing_scraper_detail_badges_for_archived_scraper : BaseDatabaseTest
{
    private BadgesModel _model = null!;
    private Scraper _scraper = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new BadgesModelBuilder(UnitOfWork).Build();
        
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
        _model.ScraperDetailArgs.ScraperId.ShouldBe(_scraper.Id);
        _model.ScraperDetailArgs.IsArchived.ShouldBe(true);
    }

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork).Build();
        _scraper.Archive();
    }    
}