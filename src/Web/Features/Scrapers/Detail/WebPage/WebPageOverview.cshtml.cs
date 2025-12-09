using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Resources;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Scrapers.Detail.WebPage;

public class WebPageOverviewModel(
    IQueryExecutor queryExecutor, 
    ICoreBus bus,
    IAuthorizationService authorizationService
) : BaseAuthorizationPageModel(authorizationService)
{
    [BindProperty]
    public ScraperWebPageArgs ScraperWebPageArgs { get; set; } = null!;
    public bool IsEmptyWebPage { get; private set; }

    public async Task<IActionResult> OnGet(
        long scraperId, 
        long scraperWebPageId
    )
    {
        if (!await IsAuthorizedAsScraperOwnerOrSuperAdmin(scraperId)) return Forbid();

        ScraperWebPageArgs =
            await queryExecutor.ExecuteSingleAsync<GetScraperWebPageArgsQuery, ScraperWebPageArgs>(
                new GetScraperWebPageArgsQuery(scraperId, scraperWebPageId));

        IsEmptyWebPage = string.IsNullOrWhiteSpace(ScraperWebPageArgs.Url);

        return Page();
    }
    
    public async Task<IActionResult> OnPost()
    {
        if (!await IsAuthorizedAsScraperOwnerOrSuperAdmin(ScraperWebPageArgs.ScraperId)) return Forbid();

        _validateHttpHeadersFormat();

        if (!ModelState.IsValid)
        {
            return PageWithUnprocessableEntityStatus422();
        }
        
        var command = new UpdateScraperWebPageCommand(ScraperWebPageArgs);
        await bus.Send(command);
        return Ok(command.Guid.ToString());

        void _validateHttpHeadersFormat()
        {
            if (string.IsNullOrWhiteSpace(ScraperWebPageArgs.HttpHeaders)) return;

            var lines = ScraperWebPageArgs.HttpHeaders.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var line in lines)
            {
                var parts = line.Split(':', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length != 2)
                {
                    ModelState.AddModelError(
                        $"{nameof(ScraperWebPageArgs)}.{nameof(ScraperWebPageArgs.HttpHeaders)}",
                        string.Format(Resource.InvalidHttpHeaderFormatErrorTemplate, line)
                    );
                }
            }
        }
    }
}