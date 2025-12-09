using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Commands;

[TestFixture]
public class when_making_scraper_public : BaseDatabaseTest
{
    private Scraper _scraper = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        var handler = new MakeScraperPublicCommandMessageHandler(new NhibernateRepository<Scraper>(UnitOfWork));

        await handler.Handle(new MakeScraperPublicCommand(_scraper.Id));
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _scraper = UnitOfWork.LoadById<Scraper>(_scraper.Id);
    }

    [Test]
    public void scraper_is_public()
    {
        _scraper.PublicStatus.ShouldBe(PublicStatus.Public);
    }

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork).Build();
    }
}