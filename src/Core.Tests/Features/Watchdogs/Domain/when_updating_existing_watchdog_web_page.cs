using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events;
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
            Name = "url.com/page_updated"
        });
    }

    [Test]
    public void watchdog_web_page_is_updated()
    {
        var watchdogWebPage = _watchdog.WebPages.ShouldHaveSingleItem();
        watchdogWebPage.Url.ShouldBe("http://url.com/page_updated");
        watchdogWebPage.Selector.ShouldBe(".selector_updated");
        watchdogWebPage.Name.ShouldBe("url.com/page_updated");
    }

    [Test]
    public void watchdog_web_page_updated_domain_event_is_raised()
    {
        RaisedDomainEvents.ShouldContain(new WatchdogWebPageUpdatedDomainEvent(_watchdog.Id, _watchdogWebPageId));
    }

    [Test]
    public void web_page_selected_html_and_scraped_on_are_reset()
    {
        var watchdogWebPage = _watchdog.WebPages.Single();
        watchdogWebPage.SelectedHtml.ShouldBe(null);
        watchdogWebPage.ScrapedOn.ShouldBe(null);
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
        _watchdog.SetSelectedHtml(_watchdogWebPageId, "<div></div>");
    }
}