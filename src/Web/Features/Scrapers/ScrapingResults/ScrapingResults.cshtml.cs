using System.ComponentModel.DataAnnotations;
using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;
using MrWatchdog.Web.Infrastructure.Authorizations;

namespace MrWatchdog.Web.Features.Scrapers.ScrapingResults;

[AllowAnonymous]
public class ScrapingResultsModel(
    IQueryExecutor queryExecutor,
    ICoreBus bus,
    IAuthorizationService authorizationService
) : BaseAuthorizationPageModel(authorizationService)
{
    public ScraperScrapingResultsArgs ScraperScrapingResultsArgs { get; private set; } = null!;
    
    [BindProperty(SupportsGet = true)]
    [StringLength(ValidationConstants.SearchTermMaxLength)]
    public string? SearchTerm { get; set; }
    
    public async Task<IActionResult> OnGet(long scraperId)
    {
        ScraperScrapingResultsArgs =
            await queryExecutor.ExecuteSingleAsync<GetScraperScrapingResultsArgsQuery, ScraperScrapingResultsArgs>(
                new GetScraperScrapingResultsArgsQuery(scraperId)
            );

        if (ScraperScrapingResultsArgs.PublicStatus == PublicStatus.Public)
        {
            return Page();
        }

        if (!User.IsAuthenticated() || !await IsAuthorizedAsScraperOwnerOrSuperAdmin(scraperId))
        {
            return Forbid();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCreateWatchdogSearch(long scraperId)
    {
        if (!User.IsAuthenticated()) return Unauthorized();

        ScraperScrapingResultsArgs =
            await queryExecutor.ExecuteSingleAsync<GetScraperScrapingResultsArgsQuery, ScraperScrapingResultsArgs>(
                new GetScraperScrapingResultsArgsQuery(scraperId)
            );

        if (!ModelState.IsValid)
        {
            return PageWithUnprocessableEntityStatus422();
        }

        if (ScraperScrapingResultsArgs.PublicStatus != PublicStatus.Public 
            && !await IsAuthorizedAsScraperOwnerOrSuperAdmin(scraperId))
        {
            return Forbid();
        }

        var command = new CreateWatchdogSearchCommand(scraperId, SearchTerm?.Trim());
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }
}