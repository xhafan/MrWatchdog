using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure;

namespace MrWatchdog.Core.Features.Watchdogs.Services;

public static class WatchdogScrapedResultsHashRefresher
{
    private const int BatchSize = 100;

    public static async Task RefreshWatchdogScrapedResultsHashes(NhibernateConfigurator nhibernateConfigurator)
    {
        await NhibernateUnitOfWorkRunner.RunAsync(
            () => new NhibernateUnitOfWork(nhibernateConfigurator),
            async newUnitOfWork =>
            {
                var session = newUnitOfWork.Session!;
                var page = 0;

                while (true)
                {
                    var watchdogs = await session.QueryOver<Watchdog>()
                        .Skip(page * BatchSize)
                        .Take(BatchSize)
                        .ListAsync();

                    if (watchdogs == null || watchdogs.Count == 0)
                        break;

                    foreach (var watchdog in watchdogs)
                    {
                        watchdog.RefreshScrapedResultsHashes();
                    }

                    await session.FlushAsync();
                    session.Clear();

                    page++;
                }
            }
        );
    }
}