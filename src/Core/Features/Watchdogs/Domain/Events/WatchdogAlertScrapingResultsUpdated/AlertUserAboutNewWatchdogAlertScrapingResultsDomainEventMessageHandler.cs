using Microsoft.Extensions.Options;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Repositories;
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

        await watchdogAlert.AlertUserAboutNewScrapingResults(emailSender, iRuntimeOptions.Value);
    }
}