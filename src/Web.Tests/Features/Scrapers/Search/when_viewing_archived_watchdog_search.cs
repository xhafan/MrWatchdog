using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Search;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Search;

[TestFixture]
public class when_viewing_archived_watchdog_search : BaseDatabaseTest
{
    private SearchModel _model = null!;
    private WatchdogSearch _watchdogSearch = null!;
    private Watchdog _watchdog = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new SearchModelBuilder(UnitOfWork).Build();
        
        await _model.OnGet(_watchdogSearch.Id);
    }

    [Test]
    public void model_is_correct()
    {
        _model.WatchdogSearchArgs.IsArchived.ShouldBe(true);
    }

    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork).Build();
        
        _watchdogSearch = new WatchdogSearchBuilder(UnitOfWork)
            .WithWatchdog(_watchdog)
            .Build();
        _watchdogSearch.Archive();

        UnitOfWork.Flush();
    }
}