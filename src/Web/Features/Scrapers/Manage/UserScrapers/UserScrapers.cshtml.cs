using System.Globalization;
using CoreBackend.Infrastructure.ActingUserAccessors;
using CoreDdd.Queries;
using CoreWeb.Features.Shared;
using MrWatchdog.Core.Features.Scrapers.Queries;

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
            new GetUserScrapersQuery(actingUserAccessor.GetActingUserId(), CultureInfo.CurrentUICulture)
        );
    }
}