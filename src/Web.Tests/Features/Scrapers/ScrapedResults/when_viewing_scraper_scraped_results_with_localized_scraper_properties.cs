using System.Globalization;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.ScrapedResults;

namespace MrWatchdog.Web.Tests.Features.Scrapers.ScrapedResults;

[TestFixture]
public class when_viewing_scraper_scraped_results_with_localized_scraper_properties : BaseDatabaseTest
{
    private ScrapedResultsModel _model = null!;
    private Scraper _scraper = null!;
    private User _user = null!;
    private CultureInfo _originalUiCulture = null!;

    [SetUp]
    public async Task Context()
    {
        _originalUiCulture = CultureInfo.CurrentUICulture;
        
        _BuildEntities();
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("cs");
        
        _model = new ScrapedResultsModelBuilder(UnitOfWork)
            .Build();
        
        await _model.OnGet(_scraper.Id);
    }

    [Test]
    public void scraper_properties_have_correct_translation()
    {
        _model.ScraperScrapedResultsArgs.ScraperName.ShouldBe("jméno scraperu");
        _model.ScraperScrapedResultsArgs.ScraperDescription.ShouldBe("popis scraperu");
        
        var webPageArgs = _model.ScraperScrapedResultsArgs.WebPages.ShouldHaveSingleItem();
        webPageArgs.Name.ShouldBe("jméno stránky");
    }
    
    [TearDown]
    public void TearDown()
    {
        CultureInfo.CurrentUICulture = _originalUiCulture;
    }    

    private void _BuildEntities()
    {
        _user = new UserBuilder(UnitOfWork).Build();

        _scraper = new ScraperBuilder(UnitOfWork)
            .WithName(
                """
                {"en": "scraper name", "cs": "jméno scraperu"}
                """
            )
            .WithDescription(
                """
                {"en": "scraper description", "cs": "popis scraperu"}
                """
            )
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = """
                       {"en": "page name", "cs": "jméno stránky"}
                       """
            })
            .WithUser(_user)
            .Build();
        var scraperWebPage = _scraper.WebPages.Single();
        _scraper.SetScrapedResults(scraperWebPage.Id, ["<div>text 1</div>", "<div>text 2</div>"]);
        _scraper.EnableWebPage(scraperWebPage.Id);
        
        UnitOfWork.Flush();
    }    
}