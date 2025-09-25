using MrWatchdog.Core.Features.Account.Domain;
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
    private Watchdog _watchdog = null!;
    private User _user = null!;

    [SetUp]
    public void Context()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .Build();
        var watchdogWebPageId = _watchdog.WebPages.Single().Id;
        _watchdog.SetScrapingResults(watchdogWebPageId, ["<div>text</div>", "<div>hello</div>"]);
        _watchdog.EnableWebPage(watchdogWebPageId);

        _user = new UserBuilder(UnitOfWork).Build();
        
        _newWatchdogAlert = new WatchdogAlertBuilder(UnitOfWork)
            .WithWatchdog(_watchdog)
            .WithUser(_user)
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

        _persistedWatchdogAlert.Watchdog.ShouldBe(_watchdog);
        _persistedWatchdogAlert.User.ShouldBe(_user);
        _persistedWatchdogAlert.SearchTerm.ShouldBe("text");
        _persistedWatchdogAlert.CurrentScrapingResults.ShouldBe(["<div>text</div>"]);
    }
}