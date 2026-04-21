using CoreBackend.Messages;
using MrWatchdog.Core.Features.Scrapers.Commands;
using System.Reflection;
using CoreBackend.Account.Features.LoginLink.Commands;

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