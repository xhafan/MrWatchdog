using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.PublicWatchdogs;

[AllowAnonymous]
public class PublicWatchdogsModel(
    IQueryExecutor queryExecutor
) : BasePageModel
{
    public IEnumerable<GetPublicWatchdogsQueryResult> PublicWatchdogs { get; private set; } = null!;
    
    public async Task OnGet()
    {
        PublicWatchdogs = await queryExecutor.ExecuteAsync<GetPublicWatchdogsQuery, GetPublicWatchdogsQueryResult>(
            new GetPublicWatchdogsQuery()
        );
    }
}