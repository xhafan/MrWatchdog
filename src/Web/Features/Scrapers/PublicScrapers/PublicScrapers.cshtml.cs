using System.Globalization;
using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Scrapers.PublicScrapers;

[AllowAnonymous]
public class PublicScrapersModel(
    IQueryExecutor queryExecutor
) : BasePageModel
{
    public IEnumerable<GetPublicScrapersQueryResult> PublicScrapers { get; private set; } = null!;
    
    public async Task OnGet()
    {
        PublicScrapers = await queryExecutor.ExecuteAsync<GetPublicScrapersQuery, GetPublicScrapersQueryResult>(
            new GetPublicScrapersQuery(CultureInfo.CurrentUICulture)
        );
    }
}