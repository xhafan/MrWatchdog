using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain.MakingScraperPublic;

[TestFixture]
public class when_making_requested_to_be_made_public_scraper_private : BaseDatabaseTest
{
    private Scraper _scraper = null!;

    [SetUp]
    public void Context()
    {
        _scraper = new ScraperBuilder().Build();
        _scraper.RequestToMakePublic();

        _scraper.MakePrivate();
    }

    [Test]
    public void public_status_is_correct()
    {
        _scraper.PublicStatus.ShouldBe(PublicStatus.Private);
    }
}