namespace MrWatchdog.Core.Features.Scrapers.Services;

public class PlaywrightScraperOptions
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string? BrowserExecutablePath { get; set; }
    public bool DisableWebPageSizeCheck { get; set; }
    public bool SaveWebPageToTempDirectory { get; set; }
}