using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogWebPageScrapingDataUpdated;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain;

[TestFixture]
public class when_updating_existing_watchdog_web_page : BaseTest
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
            SelectText = true,
            Name = "url.com/page_updated",
            HttpHeaders = """
                          User-Agent: Mozilla/5.0
                          Connection: keep-alive
                          """
        });
    }

    [Test]
    public void watchdog_web_page_is_updated()
    {
        var watchdogWebPage = _watchdog.WebPages.ShouldHaveSingleItem();
        watchdogWebPage.Url.ShouldBe("http://url.com/page_updated");
        watchdogWebPage.Selector.ShouldBe(".selector_updated");
        watchdogWebPage.SelectText.ShouldBe(true);
        watchdogWebPage.Name.ShouldBe("url.com/page_updated");

        watchdogWebPage.HttpHeaders.Count().ShouldBe(2);
        watchdogWebPage.HttpHeaders.ShouldContain(
            new WatchdogWebPageHttpHeader("User-Agent", "Mozilla/5.0"));
        watchdogWebPage.HttpHeaders.ShouldContain(new WatchdogWebPageHttpHeader("Connection", "keep-alive"));
    }

    [Test]
    public void watchdog_web_page_updated_domain_event_is_raised()
    {
        RaisedDomainEvents.ShouldContain(new WatchdogWebPageScrapingDataUpdatedDomainEvent(_watchdog.Id, _watchdogWebPageId));
    }

    [Test]
    public void web_page_scraping_results_and_scraped_on_are_reset()
    {
        var watchdogWebPage = _watchdog.WebPages.Single();
        watchdogWebPage.ScrapingResults.ShouldBeEmpty();
        watchdogWebPage.ScrapedOn.ShouldBe(null);
    }

    [Test]
    public void watchdog_web_page_is_disabled()
    {
        var watchdogWebPage = _watchdog.WebPages.Single();
        watchdogWebPage.IsEnabled.ShouldBe(false);
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
        _watchdog.SetScrapingResults(_watchdogWebPageId, ["<div>x</div>"]);
        _watchdog.EnableWebPage(_watchdogWebPageId);
    }
}