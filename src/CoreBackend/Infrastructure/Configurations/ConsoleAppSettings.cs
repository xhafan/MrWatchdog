using System.Reflection;
using CoreUtils;
using Microsoft.Extensions.Configuration;

namespace CoreBackend.Infrastructure.Configurations;

// https://stackoverflow.com/a/46437144/379279
// https://stackoverflow.com/a/40620561/379279
public static class ConsoleAppSettings
{
    public const string AspNetCoreEnvironmentVariable = "ASPNETCORE_ENVIRONMENT";

    private static IConfigurationRoot? _configuration;

    public static void Initialize(Assembly? assemblyWithUserSecrets = null)
    {
        var environmentName = Environment.GetEnvironmentVariable(AspNetCoreEnvironmentVariable) ?? "Development";

        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true);

        if (assemblyWithUserSecrets != null)
        {
            configurationBuilder.AddUserSecrets(assemblyWithUserSecrets);
        }

        var builder = configurationBuilder.AddEnvironmentVariables();

        _configuration = builder.Build();
    }

    public static IConfigurationRoot Configuration
    {
        get
        {
            Guard.Hope(_configuration != null, "Configuration is not initialized. Call ConsoleAppSettings.Initialize()");
            return _configuration;
        }
    }
}