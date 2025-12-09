using CoreUtils.Extensions;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.Core.Resources;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperWebPageScrapingFailed;

public class NotifyUserAboutScraperScrapingFailedDomainEventMessageHandler(
    IRepository<Scraper> scraperRepository,
    IEmailSender emailSender,
    IOptions<RuntimeOptions> iRuntimeOptions
) 
    : IHandleMessages<ScraperWebPageScrapingFailedDomainEvent>
{
    public async Task Handle(ScraperWebPageScrapingFailedDomainEvent domainEvent)
    {
        var scraper = await scraperRepository.LoadByIdAsync(domainEvent.ScraperId);
        var scraperWebPagesWithError = scraper.WebPages.Where(x => !string.IsNullOrWhiteSpace(x.ScrapingErrorMessage)).ToList();

        if (scraperWebPagesWithError.IsEmpty()) return;
        
        var mrWatchdogResource = Resource.MrWatchdog;
        var scraperDetailUrl = $"{iRuntimeOptions.Value.Url}{ScraperUrlConstants.ScraperDetailUrlTemplate.WithScraperId(scraper.Id)}";
        
        await emailSender.SendEmail(
            scraper.User.Email,
            $"{mrWatchdogResource}: web scraping failed for the scraper {scraper.Name}",
            $"""
             <html>
             <body>
             <p>
                 Hello,
             </p>
             <p>
                 Web scraping failed for the scraper <a href="{scraperDetailUrl}">{scraper.Name}</a>.<br>
                 Failed scraper web pages:
                 <ul>
                     {string.Join("\n", scraperWebPagesWithError
                         .Select(webPage =>
                             $"""<li><a href="{scraperDetailUrl}#scraper_web_page_{webPage.Id}">{webPage.Name}</a>, error message: {webPage.ScrapingErrorMessage}</li>""")
                     )}
                 </ul>
             </p>
             <p>
                 Kind regards,<br>
                 {mrWatchdogResource}
             </p>
             </body>
             </html>
             """
        );
    }
}