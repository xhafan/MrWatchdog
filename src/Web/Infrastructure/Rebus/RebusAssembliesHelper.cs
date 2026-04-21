using CoreBackend.Account.Features.Account.Commands;
using CoreBackend.Messages;
using MrWatchdog.Core.Features.Scrapers.Commands;
using System.Reflection;

namespace MrWatchdog.Web.Infrastructure.Rebus;

public static class RebusAssembliesHelper
{
    public static IEnumerable<Assembly> GetAssembliesWithTypesDerivedFromBaseMessage =>
    [
        typeof(Command).Assembly,
        typeof(ConfirmLoginTokenCommand).Assembly,
        typeof(ScrapeScraperCommand).Assembly
    ];
}