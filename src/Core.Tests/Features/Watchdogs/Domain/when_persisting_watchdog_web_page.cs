using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain;

[TestFixture]
public class when_persisting_watchdog_web_page : BaseDatabaseTest
{
    private Watchdog _newWatchdog = null!;
    private WatchdogWebPage _newWatchdogWebPage = null!;
    private WatchdogWebPage _persistedWatchdogWebPage = null!;

    [SetUp]
    public void Context()
    {
        _newWatchdog = new WatchdogBuilder(UnitOfWork)
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                SelectText = true,
                Name = "url.com/page"
            })
            .Build();
        _newWatchdogWebPage = _newWatchdog.WebPages.Single();
        _newWatchdog.SetScrapingResults(_newWatchdogWebPage.Id, ["<div>text</div>"]);
        
        UnitOfWork.Flush();
        UnitOfWork.Clear();

        _persistedWatchdogWebPage = UnitOfWork.LoadById<Watchdog>(_newWatchdog.Id).WebPages.Single();
    }

    [Test]
    public void persisted_watchdog_web_page_can_be_retrieved_and_has_correct_data()
    {
        _persistedWatchdogWebPage.ShouldBe(_newWatchdogWebPage);
        _persistedWatchdogWebPage.Watchdog.ShouldBe(_newWatchdog);
        _persistedWatchdogWebPage.Url.ShouldBe("http://url.com/page");
        _persistedWatchdogWebPage.Selector.ShouldBe(".selector");
        _persistedWatchdogWebPage.SelectText.ShouldBe(true);
        _persistedWatchdogWebPage.Name.ShouldBe("url.com/page");
        _persistedWatchdogWebPage.ScrapingResults.ShouldBe(["text"]);
        _persistedWatchdogWebPage.ScrapedOn.ShouldNotBeNull();
        _persistedWatchdogWebPage.ScrapedOn.Value.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
    }
}