using CoreDdd.Nhibernate.Configurations;
using CoreDdd.Nhibernate.UnitOfWorks;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using NHibernate;

namespace MrWatchdog.Web.HostedServices;

public class KickOffDueWatchdogsScrapingHostedService(
    INhibernateConfigurator nhibernateConfigurator,
    ICoreBus bus,
    IOptions<KickOffDueWatchdogsScrapingHostedServiceOptions> options,
    ILogger<KickOffDueWatchdogsScrapingHostedService> logger
) : BackgroundService
{
    private const int ScrapingIntervalInSeconds = ScrapingConstants.ScrapingIntervalInSeconds;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (options.Value.IsDisabled)
        {
            logger.LogInformation($"{nameof(KickOffDueWatchdogsScrapingHostedService)} is disabled.");
            return;
        }

        logger.LogInformation($"{nameof(KickOffDueWatchdogsScrapingHostedService)} is starting.");

        while (!cancellationToken.IsCancellationRequested)
        {
            await _KickOffDueWatchdogsScraping();

            await Task.Delay(TimeSpan.FromSeconds(ScrapingIntervalInSeconds), cancellationToken);
        }

        logger.LogInformation($"{nameof(KickOffDueWatchdogsScrapingHostedService)} is stopping.");
    }
    
    private async Task _KickOffDueWatchdogsScraping()
    {
        using var unitOfWork = new NhibernateUnitOfWork(nhibernateConfigurator);
        unitOfWork.BeginTransaction();

        var dueWatchdogsToScrape = await unitOfWork.Session
            .CreateSQLQuery(
                """
                SELECT
                    {w.*}
                FROM "Watchdog" w
                WHERE "NextScrapingOn" <= :utcNow or "NextScrapingOn" is null
                FOR UPDATE SKIP LOCKED
                """
            )
            .AddEntity("w", typeof(Watchdog))
            .SetParameter("utcNow", DateTime.UtcNow, NHibernateUtil.DateTime)
            .ListAsync<Watchdog>();

        foreach (var watchdogToScrape in dueWatchdogsToScrape)
        {
            await bus.Send(new ScrapeWatchdogCommand(watchdogToScrape.Id));
            
            watchdogToScrape.ScheduleNextScraping();
        }
    }    
}
