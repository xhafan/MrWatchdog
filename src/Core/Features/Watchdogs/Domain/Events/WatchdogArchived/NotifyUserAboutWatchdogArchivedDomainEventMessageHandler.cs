using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogArchived;

public class NotifyUserAboutWatchdogArchivedDomainEventMessageHandler(
    IRepository<Watchdog> watchdogRepository,
    IEmailSender emailSender
) 
    : IHandleMessages<WatchdogArchivedDomainEvent>
{
    public async Task Handle(WatchdogArchivedDomainEvent domainEvent)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(domainEvent.WatchdogId);

        await watchdog.NotifyUserAboutWatchdogArchived(emailSender);
    }
}