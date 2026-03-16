using System.Globalization;
using CoreDdd.Queries;
using CoreWeb.Features.Shared;
using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Core.Features.Scrapers.Queries;

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