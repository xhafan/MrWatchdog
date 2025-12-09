using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.Detail;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Detail;

[TestFixture]
public class when_viewing_scraper_detail_for_public_scraper : BaseDatabaseTest
{
    private DetailModel _model = null!;
    private Scraper _scraper = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new DetailModelBuilder(UnitOfWork)
            .Build();
        
        await _model.OnGet(_scraper.Id);
    }

    [Test]
    public void model_is_correct()
    {
        _model.ScraperDetailArgs.PublicStatus.ShouldBe(PublicStatus.Public);
    }

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork)
            .WithName("scraper name")
            .Build();
        _scraper.MakePublic();

        UnitOfWork.Flush();
    }    
}