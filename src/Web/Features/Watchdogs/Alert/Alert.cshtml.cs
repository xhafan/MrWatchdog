using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Alert;

public class AlertModel(
    IQueryExecutor queryExecutor,
    ICoreBus bus,
    IAuthorizationService authorizationService
) : BaseAuthorizationPageModel(authorizationService)
{
    public WatchdogAlertArgs WatchdogAlertArgs { get; private set; } = null!;
    public WatchdogScrapingResultsArgs WatchdogScrapingResultsArgs { get; private set; } = null!;

    [BindProperty] 
    public string? SearchTerm { get; set; }

    public async Task<IActionResult> OnGet(long watchdogAlertId)
    {
        WatchdogAlertArgs =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogAlertArgsQuery, WatchdogAlertArgs>(
                new GetWatchdogAlertArgsQuery(watchdogAlertId)
            );
        
        if (!await IsAuthorizedAsWatchdogAlertOwnerOrSuperAdmin(watchdogAlertId))
        {
            var redirectUrl = QueryHelpers.AddQueryString(
                WatchdogUrlConstants.WatchdogScrapingResultsUrlTemplate.WithWatchdogId(WatchdogAlertArgs.WatchdogId),
                new Dictionary<string, string?>
                {
                    {"searchTerm", WatchdogAlertArgs.SearchTerm}
                }
            );
            return WatchdogAlertArgs.WatchdogPublicStatus == PublicStatus.Public
                ? Redirect(redirectUrl)
                : Forbid();
        }

        SearchTerm = WatchdogAlertArgs.SearchTerm;

        WatchdogScrapingResultsArgs =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogScrapingResultsArgsQuery, WatchdogScrapingResultsArgs>(
                new GetWatchdogScrapingResultsArgsQuery(WatchdogAlertArgs.WatchdogId)
            );
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostDeleteWatchdogAlert(long watchdogAlertId)
    {
        var command = new DeleteWatchdogAlertCommand(watchdogAlertId);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }    
}