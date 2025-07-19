using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain;

[TestFixture]
public class when_updating_existing_watchdog_web_page_with_the_same_data_except_select_text_flag : BaseTest
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
            Url = "http://url.com/page",
            Selector = ".selector",
            SelectText = true,
            Name = "url.com/page"
        });
    }

    [Test]
    public void watchdog_web_page_updated_domain_event_is_raised()
    {
        RaisedDomainEvents.ShouldContain(new WatchdogWebPageScrapingDataUpdatedDomainEvent(_watchdog.Id, _watchdogWebPageId));
    }

    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder()
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                SelectText = false,
                Name = "url.com/page"
            })
            .Build();
        _watchdogWebPageId = _watchdog.WebPages.Single().Id;
    }
}