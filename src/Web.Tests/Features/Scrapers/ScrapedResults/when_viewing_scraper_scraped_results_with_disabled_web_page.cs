using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.ScrapedResults;

namespace MrWatchdog.Web.Tests.Features.Scrapers.ScrapedResults;

[TestFixture]
public class when_viewing_scraper_scraped_results_with_disabled_web_page : BaseDatabaseTest
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
    public void no_scraper_web_page_is_available()
    {
        _model.ScraperScrapedResultsArgs.WebPages.ShouldBeEmpty();
    }

    private void _BuildEntities()
    {
        _user = new UserBuilder(UnitOfWork).Build();
        
        _scraper = new ScraperBuilder(UnitOfWork)
            .WithName("scraper name")
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .WithUser(_user)
            .Build();
        var scraperWebPage = _scraper.WebPages.Single();
        _scraper.SetScrapedResults(scraperWebPage.Id, ["<div>text 1</div>", "<div>text 2</div>"]);
        _scraper.MakePublic();

        UnitOfWork.Flush();
    }    
}