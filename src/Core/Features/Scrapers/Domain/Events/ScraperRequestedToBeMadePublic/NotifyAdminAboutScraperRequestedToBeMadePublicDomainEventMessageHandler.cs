using CoreBackend.Infrastructure.EmailSenders;
using CoreBackend.Infrastructure.Rebus;
using CoreDdd.Domain.Repositories;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Infrastructure.Configurations;
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
            bus,
            iRuntimeOptions.Value,
            iEmailAddressesOptions.Value
        );
    }
}