using CoreDdd.Nhibernate.UnitOfWorks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Infrastructure.Authorizations;
using System.Diagnostics;

namespace MrWatchdog.Web.Features.Temp;

// Temporary controller for any purpose.
[ApiController]
[Authorize(Policies.SuperAdmin)]
[Route("api/[controller]/[action]")]
public class TempController(
    ICoreBus bus, 
    NhibernateUnitOfWork unitOfWork
) : ControllerBase
{
    [HttpGet]
    public string GcCollect()
    {
        long memoryInMbBefore;
        using (var proc = Process.GetCurrentProcess())
        {
            memoryInMbBefore = proc.PrivateMemorySize64 / (1024 * 1024);
        }

        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true, compacting: true);

        long memoryInMbAfter;
        using (var proc = Process.GetCurrentProcess())
        {
            memoryInMbAfter = proc.PrivateMemorySize64 / (1024 * 1024);
        }

        return $"Memory allocation before: {memoryInMbBefore} MB and after GC collection: {memoryInMbAfter} MB.";
    }

    [HttpGet]
    public async Task EnqueueLotOfScrapeWatchdogCommands()
    {
        var watchdogsToScrape = await unitOfWork.Session!.QueryOver<Watchdog>().ListAsync();

        for (var i = 0; i < 1000; i++)
        {
            foreach (var watchdogToScrape in watchdogsToScrape)
            {
                await bus.Send(new ScrapeWatchdogCommand(watchdogToScrape.Id));
            }
        }
    }
}