using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Detail;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail;

[TestFixture]
public class when_viewing_watchdog_detail_for_public_watchdog : BaseDatabaseTest
{
    private DetailModel _model = null!;
    private Watchdog _watchdog = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new DetailModelBuilder(UnitOfWork)
            .Build();
        
        await _model.OnGet(_watchdog.Id);
    }

    [Test]
    public void model_is_correct()
    {
        _model.WatchdogDetailArgs.PublicStatus.ShouldBe(PublicStatus.Public);
    }

    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithName("watchdog name")
            .Build();
        _watchdog.MakePublic();
    }    
}