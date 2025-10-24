using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;
using MrWatchdog.Web.Infrastructure.Authorizations;
using System.ComponentModel.DataAnnotations;

namespace MrWatchdog.Web.Features.Watchdogs.ScrapingResults;

[AllowAnonymous]
public class ScrapingResultsModel(
    IQueryExecutor queryExecutor,
    ICoreBus bus,
    IAuthorizationService authorizationService
) : BaseAuthorizationPageModel(authorizationService)
{
    public WatchdogScrapingResultsArgs WatchdogScrapingResultsArgs { get; private set; } = null!;
    
    [BindProperty(SupportsGet = true)]
    [StringLength(ValidationConstants.SearchTermMaxLength)]
    public string? SearchTerm { get; set; }
    
    public async Task<IActionResult> OnGet(long watchdogId)
    {
        WatchdogScrapingResultsArgs =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogScrapingResultsArgsQuery, WatchdogScrapingResultsArgs>(
                new GetWatchdogScrapingResultsArgsQuery(watchdogId)
            );

        if (WatchdogScrapingResultsArgs.PublicStatus == PublicStatus.Public)
        {
            return Page();
        }

        if (!User.IsAuthenticated() || !await IsAuthorizedAsWatchdogOwnerOrSuperAdmin(watchdogId))
        {
            return Forbid();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCreateWatchdogSearch(
        long watchdogId, 
        [FromForm] string? searchTerm
    )
    {
        if (!User.IsAuthenticated()) return Unauthorized();

        var watchdogPublicStatusArgs =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogDetailPublicStatusArgsQuery, WatchdogDetailPublicStatusArgs>(
                new GetWatchdogDetailPublicStatusArgsQuery(watchdogId)
            );

        if (watchdogPublicStatusArgs.PublicStatus != PublicStatus.Public 
            && !await IsAuthorizedAsWatchdogOwnerOrSuperAdmin(watchdogId))
        {
            return Forbid();
        }

        var command = new CreateWatchdogSearchCommand(watchdogId, searchTerm?.Trim());
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }
}