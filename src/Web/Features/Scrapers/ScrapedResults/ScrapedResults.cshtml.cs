using System.ComponentModel.DataAnnotations;
using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;
using MrWatchdog.Web.Infrastructure.Authorizations;

namespace MrWatchdog.Web.Features.Scrapers.ScrapedResults;

[AllowAnonymous]
public class ScrapedResultsModel(
    IQueryExecutor queryExecutor,
    ICoreBus bus,
    IAuthorizationService authorizationService
) : BaseAuthorizationPageModel(authorizationService)
{
    public ScraperScrapedResultsArgs ScraperScrapedResultsArgs { get; private set; } = null!;
    
    [BindProperty(SupportsGet = true)]
    [StringLength(ValidationConstants.SearchTermMaxLength)]
    public string? SearchTerm { get; set; }
    
    public async Task<IActionResult> OnGet(long scraperId)
    {
        ScraperScrapedResultsArgs =
            await queryExecutor.ExecuteSingleAsync<GetScraperScrapedResultsArgsQuery, ScraperScrapedResultsArgs>(
                new GetScraperScrapedResultsArgsQuery(scraperId)
            );

        if (ScraperScrapedResultsArgs.PublicStatus == PublicStatus.Public)
        {
            return Page();
        }

        if (!User.IsAuthenticated() || !await IsAuthorizedAsScraperOwnerOrSuperAdmin(scraperId))
        {
            return Forbid();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCreateWatchdog(long scraperId)
    {
        if (!User.IsAuthenticated()) return Unauthorized();

        ScraperScrapedResultsArgs =
            await queryExecutor.ExecuteSingleAsync<GetScraperScrapedResultsArgsQuery, ScraperScrapedResultsArgs>(
                new GetScraperScrapedResultsArgsQuery(scraperId)
            );

        if (!ModelState.IsValid)
        {
            return PageWithUnprocessableEntityStatus422();
        }

        if (ScraperScrapedResultsArgs.PublicStatus != PublicStatus.Public 
            && !await IsAuthorizedAsScraperOwnerOrSuperAdmin(scraperId))
        {
            return Forbid();
        }

        var command = new CreateWatchdogCommand(scraperId, SearchTerm?.Trim());
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }
}