using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Alerts;

[Authorize] // todo: test this in E2E test
public class AlertsModel(IQueryExecutor queryExecutor) : BasePageModel
{
    public IEnumerable<GetWatchdogAlertsQueryResult> WatchdogAlerts { get; private set; } = null!;
    
    public async Task OnGet()
    {
        WatchdogAlerts = await queryExecutor.ExecuteAsync<GetWatchdogAlertsQuery, GetWatchdogAlertsQueryResult>(new GetWatchdogAlertsQuery());
    }
}