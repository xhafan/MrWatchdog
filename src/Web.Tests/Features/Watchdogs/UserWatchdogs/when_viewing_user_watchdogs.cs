using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.UserWatchdogs;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.UserWatchdogs;

[TestFixture]
public class when_viewing_user_watchdogs : BaseDatabaseTest
{
    private Watchdog _publicWatchdog = null!;
    private UserWatchdogsModel _model = null!;
    private Watchdog _privateUserWatchdog = null!;
    private User _user = null!;
    private Watchdog _makePublicRequestedUserWatchdog = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new UserWatchdogsModelBuilder(UnitOfWork)
            .WithActingUser(_user)
            .Build();
        
        await _model.OnGet();
    }
   
    [Test]
    public void user_watchdogs_are_fetched()
    {
        _model.UserWatchdogs.Count().ShouldBe(2);
        _model.UserWatchdogs.ShouldContain(
            new GetUserWatchdogsQueryResult
            {
                WatchdogId = _privateUserWatchdog.Id, 
                WatchdogName = "private user watchdog",
                PublicStatus = PublicStatus.Private
            }
        );  
        _model.UserWatchdogs.ShouldContain(
            new GetUserWatchdogsQueryResult
            {
                WatchdogId = _makePublicRequestedUserWatchdog.Id, 
                WatchdogName = "make public requested user watchdog",
                PublicStatus = PublicStatus.MakePublicRequested
            }
        );  
    }   
    
    [Test]
    public void model_is_valid()
    {
        _model.ModelState.IsValid.ShouldBe(true);
    }
    
    private void _BuildEntities()
    {
        _publicWatchdog = new WatchdogBuilder(UnitOfWork)
            .WithName("public watchdog")
            .Build();
        _publicWatchdog.MakePublic();
        
        _user = new UserBuilder(UnitOfWork).Build();
        
        _privateUserWatchdog = new WatchdogBuilder(UnitOfWork)
            .WithName("private user watchdog")
            .WithUser(_user)
            .Build();
        
        _makePublicRequestedUserWatchdog = new WatchdogBuilder(UnitOfWork)
            .WithName("make public requested user watchdog")
            .WithUser(_user)
            .Build();
        _makePublicRequestedUserWatchdog.RequestToMakePublic();

        // ReSharper disable once UnusedVariable
        var anotherPrivateWatchdog = new WatchdogBuilder(UnitOfWork)
            .WithName("another user private watchdog")
            .Build();
        
        UnitOfWork.Flush();
    }
}