using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Manage.UserWatchdogs;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Manage.UserWatchdogs;

[TestFixture]
public class when_viewing_manage_user_watchdogs : BaseDatabaseTest
{
    private Watchdog _watchdogForUserOne = null!;
    private UserWatchdogsModel _model = null!;
    private User _userOne = null!;
    private User _userTwo = null!;
    private Watchdog _watchdogForUserTwo = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new UserWatchdogsModelBuilder(UnitOfWork)
            .WithActingUser(_userOne)
            .Build();
        
        await _model.OnGet();
    }

    [Test]
    public void model_is_correct()
    {
        _model.UserWatchdogs.ShouldContain(
            new GetUserWatchdogsQueryResult
            {
                WatchdogId = _watchdogForUserOne.Id, 
                WatchdogName = "watchdog user one",
                PublicStatus = PublicStatus.Public
            }
        );
        _model.UserWatchdogs.Any(x => x.WatchdogId == _watchdogForUserTwo.Id).ShouldBe(false);
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