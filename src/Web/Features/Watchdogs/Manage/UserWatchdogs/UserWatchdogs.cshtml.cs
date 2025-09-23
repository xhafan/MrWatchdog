using CoreDdd.Queries;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.ActingUserAccessors;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Manage.UserWatchdogs;

public class UserWatchdogsModel(
    IQueryExecutor queryExecutor,
    IActingUserAccessor actingUserAccessor
) : BasePageModel
{
    public IEnumerable<GetUserWatchdogsQueryResult> UserWatchdogs { get; private set; } = null!;
    
    public async Task OnGet()
    {
        UserWatchdogs = await queryExecutor.ExecuteAsync<GetUserWatchdogsQuery, GetUserWatchdogsQueryResult>(
            new GetUserWatchdogsQuery(actingUserAccessor.GetActingUserId())
        );
    }
}