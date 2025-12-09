using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Scrapers;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Scrapers.Detail;

public class DetailModel(
    IQueryExecutor queryExecutor,
    ICoreBus bus,
    IAuthorizationService authorizationService
) : BaseAuthorizationPageModel(authorizationService)
{
    public ScraperDetailArgs ScraperDetailArgs { get; private set; } = null!;
    
    public async Task<IActionResult> OnGet(long scraperId)
    {
        ScraperDetailArgs = await queryExecutor.ExecuteSingleAsync<GetScraperDetailArgsQuery, ScraperDetailArgs>(
            new GetScraperDetailArgsQuery(scraperId)
        );

        if (!await IsAuthorizedAsScraperOwnerOrSuperAdmin(scraperId))
        {
            return ScraperDetailArgs.PublicStatus == PublicStatus.Public 
                ? Redirect(ScraperUrlConstants.ScraperScrapingResultsUrlTemplate.WithScraperId(scraperId))
                : Forbid();
        }

        return Page();
    }
    
    public async Task<IActionResult> OnPostCreateScraperWebPage(long scraperId)
    {
        if (!await IsAuthorizedAsScraperOwnerOrSuperAdmin(scraperId)) return Forbid();

        var command = new CreateScraperWebPageCommand(scraperId);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }
    
    public async Task<IActionResult> OnPostArchiveScraper(long scraperId)
    {
        if (!await IsAuthorizedAsScraperOwnerOrSuperAdmin(scraperId)) return Forbid();

        var command = new ArchiveScraperCommand(scraperId);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }    
}