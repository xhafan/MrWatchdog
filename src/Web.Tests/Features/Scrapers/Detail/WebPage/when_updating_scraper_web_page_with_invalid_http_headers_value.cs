using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Resources;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Scrapers.Detail.WebPage;

namespace MrWatchdog.Web.Tests.Features.Scrapers.Detail.WebPage;

[TestFixture]
public class when_updating_scraper_web_page_with_invalid_http_headers_value : BaseDatabaseTest
{
    private IActionResult _actionResult = null!;
    private WebPageOverviewModel _model = null!;
    private Scraper _scraper = null!;
    private long _scraperWebPageId;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new WebPageOverviewModelBuilder(UnitOfWork)
            .WithScraperWebPageArgs(new ScraperWebPageArgs
            {
                ScraperId = _scraper.Id,
                ScraperWebPageId = _scraperWebPageId,
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page",
                HttpHeaders = """
                              User-Agent: Mozilla/5.0
                              Connection; keep-alive
                              """ // missing colon
            })
            .Build();

        _actionResult = await _model.OnPost();
    }

    [Test]
    public void action_result_is_correct()
    {
        _actionResult.ShouldBeOfType<PageResult>();
        var pageResult = (PageResult) _actionResult;
        pageResult.StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
    }

    [Test]
    public void model_is_invalid()
    {
        _model.ModelState.IsValid.ShouldBe(false);
        const string key = $"{nameof(ScraperWebPageArgs)}.{nameof(ScraperWebPageArgs.HttpHeaders)}";
        _model.ModelState.Keys.ShouldBe([key]);
        var errors = _model.ModelState[key]?.Errors;
        errors.ShouldNotBeNull();
        errors.Select(x => x.ErrorMessage).ShouldBe([
            string.Format(Resource.InvalidHttpHeaderFormatErrorTemplate, "Connection; keep-alive")
        ]);
    }    

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork)
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .Build();
        _scraperWebPageId = _scraper.WebPages.Single().Id;
    }    
}