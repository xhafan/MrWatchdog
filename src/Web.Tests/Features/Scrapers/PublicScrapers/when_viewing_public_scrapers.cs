using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.PublicScrapers;

namespace MrWatchdog.Web.Tests.Features.Scrapers.PublicScrapers;

[TestFixture]
public class when_viewing_public_scrapers : BaseDatabaseTest
{
    private Scraper _publicScraper = null!;
    private PublicScrapersModel _model = null!;
    private Scraper _anotherPrivateScraper = null!;
    private Scraper _archivedPublicScraper = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new PublicScrapersModelBuilder(UnitOfWork)
            .Build();
        
        await _model.OnGet();
    }

    [Test]
    public void public_scrapers_are_fetched()
    {
        _model.PublicScrapers.ShouldContain(
            new GetPublicScrapersQueryResult
            {
                ScraperId = _publicScraper.Id, 
                ScraperName = "public scraper",
                UserId = _publicScraper.User.Id
            }
        );
        var publicScraperIds = _model.PublicScrapers.Select(x => x.ScraperId).ToList();
        publicScraperIds.ShouldNotContain(_anotherPrivateScraper.Id);
        publicScraperIds.ShouldNotContain(_archivedPublicScraper.Id);
    }
    
    [Test]
    public void model_is_valid()
    {
        _model.ModelState.IsValid.ShouldBe(true);
    }
    
    private void _BuildEntities()
    {
        _publicScraper = new ScraperBuilder(UnitOfWork)
            .WithName("public scraper")
            .Build();
        _publicScraper.MakePublic();
        
        _anotherPrivateScraper = new ScraperBuilder(UnitOfWork)
            .WithName("another user private scraper")
            .Build();
        
        _archivedPublicScraper = new ScraperBuilder(UnitOfWork).Build();
        _archivedPublicScraper.MakePublic();
        _archivedPublicScraper.Archive();

        UnitOfWork.Flush();
    }
}