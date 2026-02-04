using CoreDdd.Nhibernate.Configurations;
using CoreDdd.Nhibernate.UnitOfWorks;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using NHibernate;

namespace MrWatchdog.Web.HostedServices;

public class KickOffDueScrapersScrapingHostedService(
    INhibernateConfigurator nhibernateConfigurator,
    ICoreBus bus,
    IOptions<KickOffDueScrapersScrapingHostedServiceOptions> options,
    ILogger<KickOffDueScrapersScrapingHostedService> logger
) : BackgroundService
{
    private IEnumerable<long>? _scraperIdsToScrape; // If null, all scrapers are considered for scraping

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (options.Value.IsDisabled)
        {
            logger.LogInformation($"{nameof(KickOffDueScrapersScrapingHostedService)} is disabled.");
            return;
        }

        logger.LogInformation($"{nameof(KickOffDueScrapersScrapingHostedService)} is starting.");

        while (!cancellationToken.IsCancellationRequested)
        {
            await _KickOffDueScrapersScraping();

            await Task.Delay(TimeSpan.FromSeconds(options.Value.DelayBetweenKickOffsInSeconds), cancellationToken);
        }

        logger.LogInformation($"{nameof(KickOffDueScrapersScrapingHostedService)} is stopping.");
    }
    
    private async Task _KickOffDueScrapersScraping()
    {
        await NhibernateUnitOfWorkRunner.RunAsync(
            () => new NhibernateUnitOfWork(nhibernateConfigurator),
            async unitOfWork =>
            {
                var sqlQuery = unitOfWork.Session!
                    .CreateSQLQuery(
                        $$"""
                          SELECT
                              {s.*}
                          FROM "Scraper" s
                          WHERE ("NextScrapingOn" <= :utcNow or "NextScrapingOn" is null)
                          and s."IsArchived" = false
                          {{(_scraperIdsToScrape == null
                              ? ""
                              : """
                                and s."Id" in (:scraperIdsToScrape)
                                """)}}
                          FOR UPDATE SKIP LOCKED
                          """
                    )
                    .AddEntity("s", typeof(Scraper))
                    .SetParameter("utcNow", DateTime.UtcNow, NHibernateUtil.DateTime);
                
                if (_scraperIdsToScrape != null)
                {
                    sqlQuery.SetParameterList("scraperIdsToScrape", _scraperIdsToScrape ?? [], NHibernateUtil.Int64);
                }

                var dueScrapersToScrape = await sqlQuery.ListAsync<Scraper>();

                foreach (var scraperToScrape in dueScrapersToScrape)
                {
                    await bus.Send(new ScrapeScraperCommand(scraperToScrape.Id));

                    scraperToScrape.ScheduleNextScraping();
                }
            }
        );
    }

    // For testing purposes only
    public void SetScraperIdsToScrape(params IEnumerable<long> scraperIdsToScrape)
    {
        _scraperIdsToScrape = scraperIdsToScrape;
    }
}
