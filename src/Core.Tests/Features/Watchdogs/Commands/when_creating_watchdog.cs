using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Commands;

[TestFixture]
public class when_creating_watchdog : BaseDatabaseTest
{
    private readonly string _watchdogName = $"watchdog name {Guid.NewGuid()}";
    private Watchdog? _newWatchdog;
    private User _user = null!;

    [SetUp]
    public async Task Context()
    {
        _user = new UserBuilder(UnitOfWork).Build();

        var handler = new CreateWatchdogCommandMessageHandler(
            new NhibernateRepository<Watchdog>(UnitOfWork),
            new UserRepository(UnitOfWork)
        );

        await handler.Handle(new CreateWatchdogCommand(_user.Id, _watchdogName));
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _newWatchdog = UnitOfWork.Session!.Query<Watchdog>()
            .SingleOrDefault(x => x.Name == _watchdogName);
    }

    [Test]
    public void new_watchdog_is_created()
    {
        _newWatchdog.ShouldNotBeNull();
        _newWatchdog.User.ShouldBe(_user);
        _newWatchdog.ScrapingIntervalInSeconds.ShouldBe(86400);
    }
}