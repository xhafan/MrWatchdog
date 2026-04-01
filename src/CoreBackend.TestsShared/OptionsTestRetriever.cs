using CoreBackend.Infrastructure.Configurations;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace CoreBackend.TestsShared;

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