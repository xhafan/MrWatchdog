using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Scrapers.Detail.WebPage;

public class WebPageScrapingResultsModel(
    IQueryExecutor queryExecutor,
    ICoreBus bus,
    IAuthorizationService authorizationService
) : BaseAuthorizationPageModel(authorizationService)
{
    [BindProperty(SupportsGet = true)]
    public long ScraperId { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public long ScraperWebPageId { get; set; }

    public ScraperWebPageScrapingResultsDto ScraperWebPageScrapingResults { get; private set; } = null!;
    
    public async Task<IActionResult> OnGet()
    {
        if (!await IsAuthorizedAsScraperOwnerOrSuperAdmin(ScraperId)) return Forbid();

        ScraperWebPageScrapingResults =
            await queryExecutor.ExecuteSingleAsync<GetScraperWebPageScrapingResultsQuery, ScraperWebPageScrapingResultsDto>(
                new GetScraperWebPageScrapingResultsQuery(ScraperId, ScraperWebPageId));
        
        return Page();
    }

    public async Task<IActionResult> OnPostScrapeScraperWebPage()
    {
        if (!await IsAuthorizedAsScraperOwnerOrSuperAdmin(ScraperId)) return Forbid();

        var command = new ScrapeScraperWebPageCommand(ScraperId, ScraperWebPageId);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }
}