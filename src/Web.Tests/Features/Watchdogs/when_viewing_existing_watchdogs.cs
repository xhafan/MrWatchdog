using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs;

namespace MrWatchdog.Web.Tests.Features.Watchdogs;

[TestFixture]
public class when_viewing_existing_watchdogs : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;
    private IndexModel _model = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new IndexModelBuilder(UnitOfWork).Build();
        
        await _model.OnGet();
    }

    [Test]
    public void model_is_correct()
    {
        _model.WatchdogScrapingResults.ShouldContain(
            new GetWatchdogsQueryResult
            {
                WatchdogId = _watchdog.Id, 
                WatchdogName = "watchdog name"
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
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithName("watchdog name")
            .Build();
    }
}