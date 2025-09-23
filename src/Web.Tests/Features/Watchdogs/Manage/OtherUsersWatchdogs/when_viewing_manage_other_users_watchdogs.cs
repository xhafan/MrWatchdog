using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Manage.OtherUsersWatchdogs;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Manage.OtherUsersWatchdogs;

[TestFixture]
public class when_viewing_manage_other_users_watchdogs : BaseDatabaseTest
{
    private Watchdog _watchdogForUserOne = null!;
    private OtherUsersWatchdogsModel _model = null!;
    private User _userOne = null!;
    private User _userTwo = null!;
    private Watchdog _watchdogForUserTwo = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new OtherUsersWatchdogsModelBuilder(UnitOfWork)
            .WithActingUser(_userOne)
            .Build();
        
        await _model.OnGet();
    }

    [Test]
    public void model_is_correct()
    {
        _model.OtherUsersWatchdogs.ShouldContain(
            new GetOtherUsersWatchdogsQueryResult
            {
                WatchdogId = _watchdogForUserTwo.Id, 
                WatchdogName = "watchdog user two",
                PublicStatus = PublicStatus.Public,
                UserId = _userTwo.Id,
                UserEmail = _userTwo.Email
            }
        );
        _model.OtherUsersWatchdogs.Any(x => x.WatchdogId == _watchdogForUserOne.Id).ShouldBe(false);
    }    
    
    private void _BuildEntities()
    {
        _userOne = new UserBuilder(UnitOfWork).Build();
        _userTwo = new UserBuilder(UnitOfWork).Build();

        _watchdogForUserOne = new WatchdogBuilder(UnitOfWork)
            .WithUser(_userOne)
            .WithName("watchdog user one")
            .Build();

        _watchdogForUserTwo = new WatchdogBuilder(UnitOfWork)
            .WithUser(_userTwo)
            .WithName("watchdog user two")
            .Build();
        _watchdogForUserTwo.MakePublic();
        
        UnitOfWork.Flush();
    }
}