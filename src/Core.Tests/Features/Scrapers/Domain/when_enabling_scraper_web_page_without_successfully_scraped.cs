using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain;

[TestFixture]
public class when_enabling_watchdog_web_page_without_successfully_scraped : BaseTest
{
    private Watchdog _watchdog = null!;
    private long _watchdogWebPageId;

    [SetUp]
    public void Context()
    {
        _BuildEntities();
    }

    [Test]
    public void exception_is_thrown()
    {
        var ex = Should.Throw<Exception>(() => _watchdog.EnableWebPage(_watchdogWebPageId));

        ex.Message.ShouldBe("Watchdog web page can be enabled only after successful scraping.");
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
    }
}