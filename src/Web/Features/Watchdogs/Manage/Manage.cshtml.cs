using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.ActingUserAccessors;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Manage;

[Authorize]
public class ManageModel(
    IQueryExecutor queryExecutor,
    IActingUserAccessor actingUserAccessor
) : BasePageModel
{
    public IEnumerable<GetWatchdogsQueryResult> Watchdogs { get; private set; } = null!;
    
    public async Task OnGet()
    {
        Watchdogs = await queryExecutor.ExecuteAsync<GetWatchdogsQuery, GetWatchdogsQueryResult>(
            new GetWatchdogsQuery(actingUserAccessor.GetActingUserId())
        );
    }
}