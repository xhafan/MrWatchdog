using CoreDdd.Nhibernate.Configurations;
using CoreDdd.Nhibernate.UnitOfWorks;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using NHibernate;

namespace MrWatchdog.Web.HostedServices;

public class WatchdogScrapingSchedulerHostedService(
    INhibernateConfigurator nhibernateConfigurator,
    ICoreBus bus,
    IOptions<WatchdogScrapingSchedulerHostedServiceOptions> options,
    ILogger<WatchdogScrapingSchedulerHostedService> logger
) : BackgroundService
{
    private const int ScheduleScrapingIntervalInSeconds = ScrapingConstants.ScrapingIntervalInSeconds;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (options.Value.IsDisabled)
        {
            logger.LogInformation($"{nameof(WatchdogScrapingSchedulerHostedService)} is disabled.");
            return;
        }

        logger.LogInformation($"{nameof(WatchdogScrapingSchedulerHostedService)} is starting.");

        while (!cancellationToken.IsCancellationRequested)
        {
            await _ScheduleWatchdogsScraping();

            await Task.Delay(TimeSpan.FromSeconds(ScheduleScrapingIntervalInSeconds), cancellationToken);
        }

        logger.LogInformation($"{nameof(WatchdogScrapingSchedulerHostedService)} is stopping.");
    }
    
    private async Task _ScheduleWatchdogsScraping()
    {
        using var unitOfWork = new NhibernateUnitOfWork(nhibernateConfigurator);
        unitOfWork.BeginTransaction();

        var watchdogsToScrape = await unitOfWork.Session
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

        foreach (var watchdogToScrape in watchdogsToScrape)
        {
            await bus.Send(new ScrapeWatchdogCommand(watchdogToScrape.Id));
        }
    }    
}
