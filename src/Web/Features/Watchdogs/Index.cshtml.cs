using System.Globalization;
using CoreBackend.Infrastructure.ActingUserAccessors;
using CoreDdd.Queries;
using CoreWeb.Features.Shared;
using MrWatchdog.Core.Features.Watchdogs.Queries;

namespace MrWatchdog.Web.Features.Watchdogs;

public class IndexModel(
    IQueryExecutor queryExecutor, 
    IActingUserAccessor actingUserAccessor
) : BasePageModel
{
    public IEnumerable<GetWatchdogsQueryResult> Watchdogs { get; private set; } = null!;
    
    public async Task OnGet()
    {
        Watchdogs = await queryExecutor.ExecuteAsync<GetWatchdogsQuery, GetWatchdogsQueryResult>(
            new GetWatchdogsQuery(actingUserAccessor.GetActingUserId(), CultureInfo.CurrentUICulture)
        );
    }
}