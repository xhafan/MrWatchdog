using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Scrapers.Detail.WebPage;

public class WebPageModel(
    IQueryExecutor queryExecutor, 
    ICoreBus bus,
    IAuthorizationService authorizationService
) : BaseAuthorizationPageModel(authorizationService)
{
    public long ScraperId { get; private set; }
    public long ScraperWebPageId { get; private set; }
    public string? ScraperWebPageName { get; private set; }

    public async Task<IActionResult> OnGet(
        long scraperId, 
        long scraperWebPageId
    )
    {
        if (!await IsAuthorizedAsScraperOwnerOrSuperAdmin(scraperId)) return Forbid();

        ScraperId = scraperId;
        ScraperWebPageId = scraperWebPageId;

        var scraperWebPageArgs =
            await queryExecutor.ExecuteSingleAsync<GetScraperWebPageArgsQuery, ScraperWebPageArgs>(
                new GetScraperWebPageArgsQuery(scraperId, scraperWebPageId));
        
        ScraperWebPageName = scraperWebPageArgs.Name;

        return Page();
    }
    
    public async Task<IActionResult> OnPostRemoveScraperWebPage(long scraperId, long scraperWebPageId)
    {
        if (!await IsAuthorizedAsScraperOwnerOrSuperAdmin(scraperId)) return Forbid();

        var command = new RemoveScraperWebPageCommand(scraperId, scraperWebPageId);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }     
}