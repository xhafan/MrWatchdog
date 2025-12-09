using CoreDdd.Queries;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.Core.Infrastructure.ActingUserAccessors;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Scrapers.Manage.UserScrapers;

public class UserScrapersModel(
    IQueryExecutor queryExecutor,
    IActingUserAccessor actingUserAccessor
) : BasePageModel
{
    public IEnumerable<GetUserScrapersQueryResult> UserScrapers { get; private set; } = null!;
    
    public async Task OnGet()
    {
        UserScrapers = await queryExecutor.ExecuteAsync<GetUserScrapersQuery, GetUserScrapersQueryResult>(
            new GetUserScrapersQuery(actingUserAccessor.GetActingUserId())
        );
    }
}