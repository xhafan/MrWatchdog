using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperRequestedToBeMadePublic;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Commands;

[TestFixture]
public class when_requesting_to_make_scraper_public : BaseDatabaseTest
{
    private Scraper _scraper = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();

        var handler = new RequestToMakeScraperPublicCommandMessageHandler(
            new NhibernateRepository<Scraper>(UnitOfWork)
        );

        await handler.Handle(new RequestToMakeScraperPublicCommand(_scraper.Id));
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _scraper = UnitOfWork.LoadById<Scraper>(_scraper.Id);
    }

    [Test]
    public void scraper_is_requested_to_make_public()
    {
        _scraper.PublicStatus.ShouldBe(PublicStatus.RequestedToBeMadePublic);
    }

    [Test]
    public void scraper_requested_to_be_made_public_domain_event_is_raised()
    {
        RaisedDomainEvents.ShouldContain(new ScraperRequestedToBeMadePublicDomainEvent(_scraper.Id));
    }

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork).Build();
    }
}