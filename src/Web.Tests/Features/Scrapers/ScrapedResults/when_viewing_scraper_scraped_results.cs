using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.ScrapedResults;

namespace MrWatchdog.Web.Tests.Features.Scrapers.ScrapedResults;

[TestFixture]
public class when_viewing_scraper_scraped_results : BaseDatabaseTest
{
    private ScrapedResultsModel _model = null!;
    private Scraper _scraper = null!;
    private User _user = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new ScrapedResultsModelBuilder(UnitOfWork)
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
        _model.ScraperScrapedResultsArgs.ScraperId.ShouldBe(_scraper.Id);
        _model.ScraperScrapedResultsArgs.ScraperName.ShouldBe("scraper name");
        _model.ScraperScrapedResultsArgs.ScraperDescription.ShouldBe("Scraper description. Choose 'Create watchdog' to receive email notification.");
        
        var webPageArgs = _model.ScraperScrapedResultsArgs.WebPages.ShouldHaveSingleItem();
        webPageArgs.Name.ShouldBe("url.com/page");
        webPageArgs.ScrapedResults.ShouldBe(["<div>text 1</div>", "<div>text 2</div>"]);
        webPageArgs.Url.ShouldBe("http://url.com/page");
        
        _model.ScraperScrapedResultsArgs.UserId.ShouldBe(_user.Id);
        _model.ScraperScrapedResultsArgs.PublicStatus.ShouldBe(PublicStatus.Public);
        _model.ScraperScrapedResultsArgs.ScrapedResultsFilteringNotSupported.ShouldBe(true);
    }

    private void _BuildEntities()
    {
        _user = new UserBuilder(UnitOfWork).Build();

        _scraper = new ScraperBuilder(UnitOfWork)
            .WithName("scraper name")
            .WithDescription(
                """
                {
                  "en": "Scraper description. Choose '${Resource_CreateWatchdog}' to receive email notification.", // a comment after trailing comma
                }
                """
            )
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .WithUser(_user)
            .WithScrapedResultsFilteringNotSupported(true)
            .Build();
        var scraperWebPage = _scraper.WebPages.Single();
        _scraper.SetScrapedResults(scraperWebPage.Id, ["<div>text 1</div>", "<div>text 2</div>"]);
        _scraper.EnableWebPage(scraperWebPage.Id);
        _scraper.MakePublic();
        
        UnitOfWork.Flush();
    }    
}