using MrWatchdog.Core.Features.Scrapers.Commands;
using System.Reflection;
using CoreBackend.Messages;

namespace MrWatchdog.Web.Infrastructure.Rebus;

public static class RebusAssembliesHelper
{
    public static IEnumerable<Assembly> GetAssembliesWithTypesDerivedFromBaseMessage =>
    [
        typeof(Command).Assembly,
        typeof(ScrapeScraperCommand).Assembly
    ];
}