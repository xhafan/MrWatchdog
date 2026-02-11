using System.Globalization;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperRequestedToBeMadePublic;

public class NotifyAdminAboutScraperRequestedToBeMadePublicDomainEventMessageHandler(
    IRepository<Scraper> scraperRepository,
    ICoreBus bus,
    IOptions<RuntimeOptions> iRuntimeOptions,
    IOptions<EmailAddressesOptions> iEmailAddressesOptions
) 
    : IHandleMessages<ScraperRequestedToBeMadePublicDomainEvent>
{
    public async Task Handle(ScraperRequestedToBeMadePublicDomainEvent domainEvent)
    {
        var scraper = await scraperRepository.LoadByIdAsync(domainEvent.ScraperId);

        await scraper.NotifyAdminAboutScraperRequestedToBeMadePublic(
            CultureInfo.GetCultureInfo("en"), // todo: load from user profile later
            bus,
            iRuntimeOptions.Value,
            iEmailAddressesOptions.Value
        );
    }
}