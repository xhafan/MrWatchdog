using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Commands;

[TestFixture]
public class when_updating_existing_scraper_web_page : BaseDatabaseTest
{
    private Scraper _scraper = null!;
    private long _scraperWebPageId;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        var handler = new UpdateScraperWebPageCommandMessageHandler(new NhibernateRepository<Scraper>(UnitOfWork));

        await handler.Handle(new UpdateScraperWebPageCommand(new ScraperWebPageArgs
        {
            ScraperId = _scraper.Id,
            ScraperWebPageId = _scraperWebPageId,
            Url = "http://url.com/page_updated",
            Selector = ".selector_updated",
            Name = "url.com/page_updated"
        }));
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _scraper = UnitOfWork.LoadById<Scraper>(_scraper.Id);
    }

    [Test]
    public void scraper_web_page_is_updated()
    {
        var scraperWebPage = _scraper.WebPages.ShouldHaveSingleItem();
        scraperWebPage.Url.ShouldBe("http://url.com/page_updated");
        scraperWebPage.Selector.ShouldBe(".selector_updated");
        scraperWebPage.Name.ShouldBe("url.com/page_updated");
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