using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain.MakingScraperPublic;

[TestFixture]
public class when_requesting_to_make_scraper_public_with_the_scraper_already_public
{
    private Scraper _scraper = null!;

    [SetUp]
    public void Context()
    {
        _scraper = new ScraperBuilder().Build();
        _scraper.MakePublic();
    }

    [Test]
    public void exception_is_thrown()
    {
        var ex = Should.Throw<Exception>(() => _scraper.RequestToMakePublic());

        ex.Message.ShouldBe("Scraper is already public.");
    }
}