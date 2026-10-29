using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MrWatchdog.Web.E2E.Tests;

internal static class TestLogging
{
    private static bool IsVerbose => Environment.GetEnvironmentVariable("MRWATCHDOG_VERBOSE_TESTS") == "1";

    public static void Configure(ILoggingBuilder logging)
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(IsVerbose ? LogLevel.Trace : LogLevel.Warning);
    }

    public static IConfigurationBuilder AddTestLogging(this IConfigurationBuilder configuration)
    {
        var level = IsVerbose ? "Verbose" : "Warning";
        return configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Serilog:MinimumLevel:Default"] = level,
            ["Serilog:MinimumLevel:Override:Microsoft.AspNetCore"] = level,
            ["Serilog:MinimumLevel:Override:Rebus"] = level,
            ["Serilog:MinimumLevel:Override:Rebus.Bus"] = level
        });
    }
}
