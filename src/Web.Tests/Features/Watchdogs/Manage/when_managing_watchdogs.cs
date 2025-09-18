using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Manage;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Manage;

[TestFixture]
public class when_managing_watchdogs : BaseDatabaseTest
{
    private Watchdog _watchdogForUserOne = null!;
    private ManageModel _model = null!;
    private User _userOne = null!;
    private User _userTwo = null!;
    private Watchdog _watchdogForUserTwo = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new ManageModelBuilder(UnitOfWork)
            .WithActingUser(_userOne)
            .Build();
        
        await _model.OnGet();
    }

    [Test]
    public void model_is_correct()
    {
        _model.Watchdogs.ShouldContain(
            new GetUserWatchdogsQueryResult
            {
                WatchdogId = _watchdogForUserOne.Id, 
                WatchdogName = "watchdog user one",
                PublicStatus = PublicStatus.Public
            }
        );
        _model.Watchdogs.ShouldNotContain(
            new GetUserWatchdogsQueryResult
            {
                WatchdogId = _watchdogForUserTwo.Id, 
                WatchdogName = "watchdog user two",
                PublicStatus = PublicStatus.Private
            }
        );
    }    
    
    private void _BuildEntities()
    {
        _userOne = new UserBuilder(UnitOfWork).Build();
        _userTwo = new UserBuilder(UnitOfWork).Build();

        _watchdogForUserOne = new WatchdogBuilder(UnitOfWork)
            .WithUser(_userOne)
            .WithName("watchdog user one")
            .Build();
        _watchdogForUserOne.MakePublic();

        _watchdogForUserTwo = new WatchdogBuilder(UnitOfWork)
            .WithUser(_userTwo)
            .WithName("watchdog user two")
            .Build();
        
        UnitOfWork.Flush();
    }
}