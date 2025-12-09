using Microsoft.Extensions.Options;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Scrapers.Domain.Events.WatchdogSearchScrapingResultsUpdated;

public class NotifyUserAboutNewWatchdogSearchScrapingResultsDomainEventMessageHandler(
    IRepository<WatchdogSearch> watchdogSearchRepository,
    IEmailSender emailSender,
    IOptions<RuntimeOptions> iRuntimeOptions
) 
    : IHandleMessages<WatchdogSearchScrapingResultsUpdatedDomainEvent>
{
    public async Task Handle(WatchdogSearchScrapingResultsUpdatedDomainEvent domainEvent)
    {
        var watchdogSearch = await watchdogSearchRepository.LoadByIdAsync(domainEvent.WatchdogSearchId);

        await watchdogSearch.NotifyUserAboutNewScrapingResults(emailSender, iRuntimeOptions.Value);
    }
}