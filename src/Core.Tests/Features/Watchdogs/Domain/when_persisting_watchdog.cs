using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain;

[TestFixture]
public class when_persisting_watchdog : BaseDatabaseTest
{
    private Watchdog _newWatchdog = null!;
    private Watchdog? _persistedWatchdog;

    [SetUp]
    public void Context()
    {
        _newWatchdog = new WatchdogBuilder(UnitOfWork).Build();
        
        UnitOfWork.Flush();
        UnitOfWork.Clear();

        _persistedWatchdog = UnitOfWork.Get<Watchdog>(_newWatchdog.Id);
    }

    [Test]
    public void persisted_watchdog_can_be_retrieved_and_has_correct_data()
    {
        _persistedWatchdog.ShouldNotBeNull();
        _persistedWatchdog.ShouldBe(_newWatchdog);

        _persistedWatchdog.Name.ShouldBe(_newWatchdog.Name);
    }
}