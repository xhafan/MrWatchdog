using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Detail.Badges;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.Badges;

[TestFixture]
public class when_viewing_watchdog_detail_badges_for_watchdog_with_make_public_requested : BaseDatabaseTest
{
    private BadgesModel _model = null!;
    private Watchdog _watchdog = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new BadgesModelBuilder(UnitOfWork).Build();
        
        await _model.OnGet(_watchdog.Id);
    }

    [Test]
    public void model_is_correct()
    {
        _model.WatchdogDetailPublicStatusArgs.WatchdogId.ShouldBe(_watchdog.Id);
        _model.WatchdogDetailPublicStatusArgs.PublicStatus.ShouldBe(PublicStatus.MakePublicRequested);
    }

    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork).Build();
        _watchdog.RequestToMakePublic();
    }    
}