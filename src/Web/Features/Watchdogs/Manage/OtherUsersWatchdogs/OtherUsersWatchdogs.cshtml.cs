using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.ActingUserAccessors;
using MrWatchdog.Web.Features.Shared;
using MrWatchdog.Web.Infrastructure.Authorizations;

namespace MrWatchdog.Web.Features.Watchdogs.Manage.OtherUsersWatchdogs;

[Authorize(Policies.SuperAdmin)]
public class OtherUsersWatchdogsModel(
    IQueryExecutor queryExecutor,
    IActingUserAccessor actingUserAccessor
) : BasePageModel
{
    public IEnumerable<GetOtherUsersWatchdogsQueryResult> OtherUsersWatchdogs { get; private set; } = null!;
    
    public async Task OnGet()
    {
        OtherUsersWatchdogs = await queryExecutor.ExecuteAsync<GetOtherUsersWatchdogsQuery, GetOtherUsersWatchdogsQueryResult>(
            new GetOtherUsersWatchdogsQuery(actingUserAccessor.GetActingUserId())
        );
    }
}