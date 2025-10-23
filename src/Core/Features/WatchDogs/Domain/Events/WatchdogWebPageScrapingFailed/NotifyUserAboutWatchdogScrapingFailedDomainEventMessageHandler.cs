using CoreUtils.Extensions;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.Core.Resources;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogWebPageScrapingFailed;

public class NotifyUserAboutWatchdogScrapingFailedDomainEventMessageHandler(
    IRepository<Watchdog> watchdogRepository,
    IEmailSender emailSender,
    IOptions<RuntimeOptions> iRuntimeOptions
) 
    : IHandleMessages<WatchdogWebPageScrapingFailedDomainEvent>
{
    public async Task Handle(WatchdogWebPageScrapingFailedDomainEvent domainEvent)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(domainEvent.WatchdogId);
        var watchdogWebPagesWithError = watchdog.WebPages.Where(x => !string.IsNullOrWhiteSpace(x.ScrapingErrorMessage)).ToList();

        if (watchdogWebPagesWithError.IsEmpty()) return;
        
        var mrWatchdogResource = Resource.MrWatchdog;
        var watchdogDetailUrl = $"{iRuntimeOptions.Value.Url}{WatchdogUrlConstants.WatchdogDetailUrlTemplate.WithWatchdogId(watchdog.Id)}";
        
        await emailSender.SendEmail(
            watchdog.User.Email,
            $"{mrWatchdogResource}: web scraping failed for the watchdog {watchdog.Name}",
            $"""
             <html>
             <body>
             <p>
                 Hello,
             </p>
             <p>
                 Web scraping failed for the watchdog <a href="{watchdogDetailUrl}">{watchdog.Name}</a>.<br>
                 Failed watchdog web pages:
                 <ul>
                     {string.Join("\n", watchdogWebPagesWithError
                         .Select(webPage =>
                             $"""<li><a href="{watchdogDetailUrl}#watchdog_web_page_{webPage.Id}">{webPage.Name}</a>, error message: {webPage.ScrapingErrorMessage}</li>""")
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