using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain;

[TestFixture]
public class when_persisting_watchdog_alert : BaseDatabaseTest
{
    private WatchdogAlert _newWatchdogAlert = null!;
    private WatchdogAlert? _persistedWatchdogAlert;

    [SetUp]
    public void Context()
    {
        var watchdog = new WatchdogBuilder(UnitOfWork)
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .Build();
        watchdog.SetSelectedElements(watchdog.WebPages.Single().Id, ["<div>text</div>", "<div>hello</div>"]);
        _newWatchdogAlert = new WatchdogAlertBuilder(UnitOfWork)
            .WithWatchdog(watchdog)
            .WithSearchTerm("text")
            .Build();
        
        UnitOfWork.Flush();
        UnitOfWork.Clear();

        _persistedWatchdogAlert = UnitOfWork.Get<WatchdogAlert>(_newWatchdogAlert.Id);
    }

    [Test]
    public void persisted_watchdog_alert_can_be_retrieved_and_has_correct_data()
    {
        _persistedWatchdogAlert.ShouldNotBeNull();
        _persistedWatchdogAlert.ShouldBe(_newWatchdogAlert);

        _persistedWatchdogAlert.Watchdog.ShouldBe(_newWatchdogAlert.Watchdog);
        _persistedWatchdogAlert.SearchTerm.ShouldBe("text");
        _persistedWatchdogAlert.PreviousScrapedResults.ShouldBeEmpty();
        _persistedWatchdogAlert.CurrentScrapedResults.ShouldBe(["<div>text</div>"]);
    }
}