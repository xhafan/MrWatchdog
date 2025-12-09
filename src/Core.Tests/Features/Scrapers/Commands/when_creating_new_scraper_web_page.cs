using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Commands;

[TestFixture]
public class when_creating_new_scraper_web_page : BaseDatabaseTest
{
    private Scraper _scraper = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        var handler = new CreateScraperWebPageCommandMessageHandler(new NhibernateRepository<Scraper>(UnitOfWork));

        await handler.Handle(new CreateScraperWebPageCommand(_scraper.Id));
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _scraper = UnitOfWork.LoadById<Scraper>(_scraper.Id);
    }

    [Test]
    public void empty_scraper_web_page_is_created()
    {
        var scraperWebPage = _scraper.WebPages.ShouldHaveSingleItem();
        scraperWebPage.Url.ShouldBe(null);
        scraperWebPage.Selector.ShouldBe(null);
        scraperWebPage.Name.ShouldBe(null);
    }

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork).Build();
    }
}