using CoreUtils.Extensions;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.Core.Resources;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogAlertScrapingResultsUpdated;

public class AlertUserAboutNewWatchdogAlertScrapingResultsDomainEventMessageHandler(
    IRepository<WatchdogAlert> watchdogAlertRepository,
    IEmailSender emailSender,
    IOptions<RuntimeOptions> iRuntimeOptions
) 
    : IHandleMessages<WatchdogAlertScrapingResultsUpdatedDomainEvent>
{
    public async Task Handle(WatchdogAlertScrapingResultsUpdatedDomainEvent domainEvent)
    {
        var watchdogAlert = await watchdogAlertRepository.LoadByIdAsync(domainEvent.WatchdogAlertId);
        
        var user = watchdogAlert.User;
        var newScrapingResults = watchdogAlert.ScrapingResultsToAlertAbout.ToList();
        
        if (newScrapingResults.IsEmpty()) return;

        var mrWatchdogResource = Resource.MrWatchdog;
        
        await emailSender.SendEmail(
            user.Email,
            $"New results for {mrWatchdogResource} alert {watchdogAlert.Watchdog.Name}",
            $"""
              <p>
                  Hello,
              </p>
              <p>
                  We've found new results for your alert <a href="{iRuntimeOptions.Value.Url}{WatchdogUrlConstants.WatchdogAlertUrl.Replace(WatchdogUrlConstants.WatchdogAlertIdVariable, watchdogAlert.Id.ToString())}">{watchdogAlert.Watchdog.Name}</a>:
              </p>
              <ul>
                  {string.Join("\n    ", watchdogAlert.ScrapingResultsToAlertAbout.Select(scrapingResult => $"<li>{scrapingResult}</li>"))}
              </ul>
              <p>
                  Best regards,<br>
                  {mrWatchdogResource}
              </p>
              """
        );

        watchdogAlert.ClearScrapingResultsToAlertAbout();
    }
}