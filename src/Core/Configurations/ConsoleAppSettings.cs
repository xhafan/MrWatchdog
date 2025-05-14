using Microsoft.Extensions.Configuration;

namespace MrWatchdog.Core.Configurations;

public static class ConsoleAppSettings
{
    // https://stackoverflow.com/a/46437144/379279
    // https://stackoverflow.com/a/40620561/379279
    static ConsoleAppSettings()
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
            .AddEnvironmentVariables();
        Configuration = builder.Build();
    }

    public static IConfigurationRoot Configuration { get; }
}