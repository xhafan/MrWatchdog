using Microsoft.Extensions.Options;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogRequestedToBeMadePublic;

public class NotifyAdminsAboutWatchdogRequestedToBeMadePublicDomainEventMessageHandler(
    IRepository<Watchdog> watchdogRepository,
    IEmailSender emailSender,
    IOptions<RuntimeOptions> iRuntimeOptions,
    IOptions<EmailAddressesOptions> iEmailAddressesOptions
) 
    : IHandleMessages<WatchdogRequestedToBeMadePublicDomainEvent>
{
    public async Task Handle(WatchdogRequestedToBeMadePublicDomainEvent domainEvent)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(domainEvent.WatchdogId);

        await watchdog.NotifyAdminsAboutWatchdogRequestedToBeMadePublic(
            emailSender,
            iRuntimeOptions.Value,
            iEmailAddressesOptions.Value
        );
    }
}