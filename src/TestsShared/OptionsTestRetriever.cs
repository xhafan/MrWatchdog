using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Infrastructure.Configurations;

namespace MrWatchdog.TestsShared;

public static class OptionsTestRetriever
{
    public static IOptions<TOptions> Retrieve<TOptions>(string? configurationSectionKey = null)
        where TOptions : class, new()
    {
        var iOptions = A.Fake<IOptions<TOptions>>();
        var options = new TOptions();
        configurationSectionKey ??= typeof(TOptions).Name.Replace("Options", "");
        var configurationSection = ConsoleAppSettings.Configuration.GetSection(configurationSectionKey);
        configurationSection.Bind(options);
        A.CallTo(() => iOptions.Value).Returns(options);
        return iOptions;
    }
}