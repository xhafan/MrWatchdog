using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogSearchArchived;

public class NotifyUserAboutWatchdogSearchArchivedDomainEventMessageHandler(
    IRepository<WatchdogSearch> watchdogSearchRepository,
    IEmailSender emailSender
) 
    : IHandleMessages<WatchdogSearchArchivedDomainEvent>
{
    public async Task Handle(WatchdogSearchArchivedDomainEvent domainEvent)
    {
        var watchdogSearch = await watchdogSearchRepository.LoadByIdAsync(domainEvent.WatchdogSearchId);

        await watchdogSearch.NotifyUserAboutWatchdogSearchArchived(emailSender);
    }
}