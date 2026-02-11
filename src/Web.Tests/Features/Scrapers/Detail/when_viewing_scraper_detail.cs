using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.Detail;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Detail;

[TestFixture]
public class when_viewing_scraper_detail : BaseDatabaseTest
{
    private DetailModel _model = null!;
    private Scraper _scraper = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new DetailModelBuilder(UnitOfWork).Build();
        
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
        _model.ScraperDetailArgs.WebPageIds.ShouldBeEmpty();
        _model.ScraperDetailArgs.Name.ShouldBe("scraper name");
        _model.ScraperDetailArgs.PublicStatus.ShouldBe(PublicStatus.RequestedToBeMadePublic);
        _model.ScraperDetailArgs.UserId.ShouldBe(_scraper.User.Id);
        _model.ScraperDetailArgs.UserEmail.ShouldBe(_scraper.User.Email);
    }

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork)
            .WithName("""
                      { "en": "scraper name" }
                      """)
            .Build();
        _scraper.RequestToMakePublic();
    }    
}