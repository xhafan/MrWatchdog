using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.Detail.Actions;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Detail.Actions;

[TestFixture]
public class when_viewing_scraper_detail_actions_for_public_scraper : BaseDatabaseTest
{
    private ActionsModel _model = null!;
    private Scraper _scraper = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new ActionsModelBuilder(UnitOfWork).Build();
        
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
        _model.ScraperDetailPublicStatusArgs.ScraperId.ShouldBe(_scraper.Id);
        _model.ScraperDetailPublicStatusArgs.PublicStatus.ShouldBe(PublicStatus.Public);
    }

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork).Build();
        _scraper.MakePublic();
    }    
}