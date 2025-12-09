using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.Core.Infrastructure.ActingUserAccessors;
using MrWatchdog.Web.Features.Shared;
using MrWatchdog.Web.Infrastructure.Authorizations;

namespace MrWatchdog.Web.Features.Scrapers.Manage.OtherUsersScrapers;

[Authorize(Policies.SuperAdmin)]
public class OtherUsersScrapersModel(
    IQueryExecutor queryExecutor,
    IActingUserAccessor actingUserAccessor
) : BasePageModel
{
    public IEnumerable<GetOtherUsersScrapersQueryResult> OtherUsersScrapers { get; private set; } = null!;
    
    public async Task OnGet()
    {
        OtherUsersScrapers = await queryExecutor.ExecuteAsync<GetOtherUsersScrapersQuery, GetOtherUsersScrapersQueryResult>(
            new GetOtherUsersScrapersQuery(actingUserAccessor.GetActingUserId())
        );
    }
}