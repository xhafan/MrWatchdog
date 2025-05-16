using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Commands;

[TestFixture]
public class when_creating_watchdog : BaseDatabaseTest
{
    private readonly string _watchdogName = $"watchdog name {Guid.NewGuid()}";
    private Watchdog? _newWatchdog;

    [SetUp]
    public async Task Context()
    {
        var handler = new CreateWatchdogCommandMessageHandler(new NhibernateRepository<Watchdog>(UnitOfWork));

        await handler.Handle(new CreateWatchdogCommand(_watchdogName));
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _newWatchdog = UnitOfWork.Session.Query<Watchdog>()
            .SingleOrDefault(x => x.Name == _watchdogName);
    }

    [Test]
    public void new_watchdog_is_created()
    {
        _newWatchdog.ShouldNotBeNull();
    }
}