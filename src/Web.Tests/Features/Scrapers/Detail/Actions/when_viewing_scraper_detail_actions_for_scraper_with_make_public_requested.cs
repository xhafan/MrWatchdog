using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.Detail.Actions;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Detail.Actions;

[TestFixture]
public class when_viewing_scraper_detail_actions_for_scraper_with_make_public_requested : BaseDatabaseTest
{
    private ActionsModel _model = null!;
    private Scraper _scraper = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new ActionsModelBuilder(UnitOfWork).Build();
        
        await _model.OnGet(_scraper.Id);
    }

    [Test]
    public void model_is_correct()
    {
        _model.ScraperDetailPublicStatusArgs.ScraperId.ShouldBe(_scraper.Id);
        _model.ScraperDetailPublicStatusArgs.PublicStatus.ShouldBe(PublicStatus.RequestedToBeMadePublic);
    }

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork).Build();
        _scraper.RequestToMakePublic();
    } 
}