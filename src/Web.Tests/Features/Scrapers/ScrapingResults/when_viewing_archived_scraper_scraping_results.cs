using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.ScrapingResults;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.ScrapingResults;

[TestFixture]
public class when_viewing_archived_watchdog_scraping_results : BaseDatabaseTest
{
    private ScrapingResultsModel _model = null!;
    private Watchdog _watchdog = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new ScrapingResultsModelBuilder(UnitOfWork)
            .Build();
        
        await _model.OnGet(_watchdog.Id);
    }

    [Test]
    public void model_is_correct()
    {
        _model.WatchdogScrapingResultsArgs.IsArchived.ShouldBe(true);
    }

    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork).Build();
        _watchdog.Archive();
    }    
}