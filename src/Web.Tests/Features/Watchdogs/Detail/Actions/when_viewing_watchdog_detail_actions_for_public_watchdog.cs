using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Detail.Actions;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.Actions;

[TestFixture]
public class when_viewing_watchdog_detail_actions_for_public_watchdog : BaseDatabaseTest
{
    private ActionsModel _model = null!;
    private Watchdog _watchdog = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new ActionsModelBuilder(UnitOfWork).Build();
        
        await _model.OnGet(_watchdog.Id);
    }

    [Test]
    public void model_is_correct()
    {
        _model.WatchdogDetailPublicStatusArgs.WatchdogId.ShouldBe(_watchdog.Id);
        _model.WatchdogDetailPublicStatusArgs.MakePublicRequested.ShouldBe(false);
        _model.WatchdogDetailPublicStatusArgs.Public.ShouldBe(true);
    }

    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork).Build();
        _watchdog.MakePublic();
    }    
}