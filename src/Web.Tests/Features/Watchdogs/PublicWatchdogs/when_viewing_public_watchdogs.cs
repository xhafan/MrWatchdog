using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.PublicWatchdogs;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.PublicWatchdogs;

[TestFixture]
public class when_viewing_public_watchdogs : BaseDatabaseTest
{
    private Watchdog _publicWatchdog = null!;
    private PublicWatchdogsModel _model = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new PublicWatchdogsModelBuilder(UnitOfWork)
            .Build();
        
        await _model.OnGet();
    }

    [Test]
    public void public_watchdogs_are_fetched()
    {
        _model.PublicWatchdogs.Count().ShouldBe(1);
        _model.PublicWatchdogs.ShouldContain(
            new GetPublicWatchdogsQueryResult
            {
                WatchdogId = _publicWatchdog.Id, 
                WatchdogName = "public watchdog",
                UserId = _publicWatchdog.User.Id
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
        
        // ReSharper disable once UnusedVariable
        var anotherPrivateWatchdog = new WatchdogBuilder(UnitOfWork)
            .WithName("another user private watchdog")
            .Build();
        
        UnitOfWork.Flush();
    }
}