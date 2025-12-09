using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Commands;

[TestFixture]
public class when_creating_scraper : BaseDatabaseTest
{
    private readonly string _scraperName = $"scraper name {Guid.NewGuid()}";
    private Scraper? _newScraper;
    private User _user = null!;

    [SetUp]
    public async Task Context()
    {
        _user = new UserBuilder(UnitOfWork).Build();

        var handler = new CreateScraperCommandMessageHandler(
            new NhibernateRepository<Scraper>(UnitOfWork),
            new UserRepository(UnitOfWork)
        );

        await handler.Handle(new CreateScraperCommand(_user.Id, _scraperName));
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _newScraper = UnitOfWork.Session!.Query<Scraper>()
            .SingleOrDefault(x => x.Name == _scraperName);
    }

    [Test]
    public void new_scraper_is_created()
    {
        _newScraper.ShouldNotBeNull();
        _newScraper.User.ShouldBe(_user);
        _newScraper.ScrapingIntervalInSeconds.ShouldBe(86400);
    }
}