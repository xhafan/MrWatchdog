using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Manage;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Manage;

[TestFixture]
public class when_managing_watchdogs : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;
    private ManageModel _model = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new ManageModelBuilder(UnitOfWork).Build();
        
        await _model.OnGet();
    }

    [Test]
    public void model_is_correct()
    {
        _model.WatchdogResults.ShouldBe([
            new GetWatchdogsQueryResult
            {
                Id = _watchdog.Id, 
                Name = "watchdog name"
            }
        ]);
    }    
    
    [Test]
    public void model_is_valid()
    {
        _model.ModelState.IsValid.ShouldBe(true);
    }
    
    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithName("watchdog name")
            .Build();
    }
}