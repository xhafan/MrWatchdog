namespace MrWatchdog.Core.Features.Scrapers.Domain;

public static class ScrapingConstants
{
    public const int MinimumScrapingIntervalInSeconds = 7200; // 2h - once the server gets better mail sending reputation, this can be lowered

    public const int WebPageSizeLimitInMegaBytes = 10;
}