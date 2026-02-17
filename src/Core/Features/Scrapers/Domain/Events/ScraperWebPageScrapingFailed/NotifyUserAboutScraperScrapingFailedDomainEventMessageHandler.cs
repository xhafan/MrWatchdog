using CoreUtils.Extensions;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.Core.Resources;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperWebPageScrapingFailed;

public class NotifyUserAboutScraperScrapingFailedDomainEventMessageHandler(
    IRepository<Scraper> scraperRepository,
    ICoreBus bus,
    IOptions<RuntimeOptions> iRuntimeOptions
) 
    : IHandleMessages<ScraperWebPageScrapingFailedDomainEvent>
{
    public async Task Handle(ScraperWebPageScrapingFailedDomainEvent domainEvent)
    {
        var scraper = await scraperRepository.LoadByIdAsync(domainEvent.ScraperId);
        var scraperWebPagesWithError = scraper.WebPages.Where(x => !string.IsNullOrWhiteSpace(x.ScrapingErrorMessage)).ToList();

        if (scraperWebPagesWithError.IsEmpty()) return;
        
        var userCulture = scraper.User.Culture;

        var mrWatchdogResource = ResourceHelper.GetString(nameof(Resource.MrWatchdog), userCulture);
        var scraperDetailUrl = $"{iRuntimeOptions.Value.Url}{ScraperUrlConstants.ScraperDetailUrlTemplate.WithScraperId(scraper.Id)}";

        var localizedScraperName = scraper.GetLocalizedName(userCulture);

        await bus.Send(new SendEmailCommand(
            scraper.User.Email,
            string.Format(ResourceHelper.GetString(nameof(Resource.WebScrapingFailedEmailSubject), userCulture), mrWatchdogResource, localizedScraperName),
            string.Format(
                ResourceHelper.GetString(nameof(Resource.WebScrapingFailedEmailBody), userCulture), 
                scraperDetailUrl, 
                localizedScraperName,
                string.Join("\n", scraperWebPagesWithError
                    .Select(webPage =>
                    {
                        var webPageLocalizedName = webPage.GetLocalizedName(userCulture);
                        return $"""<li><a href="{scraperDetailUrl}#scraper_web_page_{webPage.Id}">{webPageLocalizedName}</a>: {webPage.ScrapingErrorMessage}</li>""";
                    })
                ),
                mrWatchdogResource
            )
        ));
    }
}