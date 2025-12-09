using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain;

[TestFixture]
public class when_updating_existing_watchdog_web_page_with_previously_having_scraping_error_message : BaseTest
{
    private Watchdog _watchdog = null!;
    private long _watchdogWebPageId;

    [SetUp]
    public void Context()
    {
        _BuildEntities();
        
        _watchdog.UpdateWebPage(new WatchdogWebPageArgs
        {
            WatchdogWebPageId = _watchdogWebPageId,
            Url = "http://url.com/page_updated",
            Selector = ".selector_updated",
            Name = "url.com/page_updated"
        });
    }

    [Test]
    public void web_page_scraping_error_message_is_reset()
    {
        var watchdogWebPage = _watchdog.WebPages.Single();
        watchdogWebPage.ScrapingErrorMessage.ShouldBe(null);
    }

    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder()
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .Build();
        _watchdogWebPageId = _watchdog.WebPages.Single().Id;
        _watchdog.SetScrapingErrorMessage(_watchdogWebPageId, "Network error");
    }
}