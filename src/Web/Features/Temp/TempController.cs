using CoreDdd.Nhibernate.UnitOfWorks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Infrastructure.Authorizations;
using System.Diagnostics;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.EmailSenders;

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
    [HttpPost]
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

    [HttpPost]
    public async Task EnqueueLotOfScrapeScraperCommands()
    {
        var scrapersToScrape = await unitOfWork.Session!.QueryOver<Scraper>().ListAsync();

        for (var i = 0; i < 1000; i++)
        {
            foreach (var scraperToScrape in scrapersToScrape)
            {
                await bus.Send(new ScrapeScraperCommand(scraperToScrape.Id));
            }
        }
    }

    [HttpPost]
    public async Task SendJustSpamRemovalEmail()
    {
        await bus.Send(new SendEmailCommand(
            "remove+46.224.68.58@db.justspam.org",
            "remove 46.224.68.58 :x:900adc2ee089dc1:x:",
            "Please remove 46.224.68.58 from your blacklist."
        ));
    }
}