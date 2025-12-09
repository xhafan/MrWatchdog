using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Scrapers.Detail.Actions;

public class ActionsModel(
    IQueryExecutor queryExecutor, 
    ICoreBus bus,
    IAuthorizationService authorizationService
) : BaseAuthorizationPageModel(authorizationService)
{
    [BindProperty]
    public ScraperDetailPublicStatusArgs ScraperDetailPublicStatusArgs { get; set; } = null!;
    
    public async Task<IActionResult> OnGet(long scraperId)
    {
        if (!await IsAuthorizedAsScraperOwnerOrSuperAdmin(scraperId)) return Forbid();

        ScraperDetailPublicStatusArgs =
            await queryExecutor.ExecuteSingleAsync<GetScraperDetailPublicStatusArgsQuery, ScraperDetailPublicStatusArgs>(
                new GetScraperDetailPublicStatusArgsQuery(scraperId));

        return Page();
    }
    
    public async Task<IActionResult> OnPostRequestToMakePublic(long scraperId)
    {
        if (!await IsAuthorizedAsScraperOwnerOrSuperAdmin(scraperId)) return Forbid();

        var command = new RequestToMakeScraperPublicCommand(scraperId);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    } 
    
    public async Task<IActionResult> OnPostMakePrivate(long scraperId)
    {
        if (!await IsAuthorizedAsScraperOwnerOrSuperAdmin(scraperId)) return Forbid();

        var command = new MakeScraperPrivateCommand(scraperId);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }
    
    public async Task<IActionResult> OnPostMakePublic(long scraperId)
    {
        if (!await IsAuthorizedAsSuperAdmin()) return Forbid();

        var command = new MakeScraperPublicCommand(scraperId);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }
}