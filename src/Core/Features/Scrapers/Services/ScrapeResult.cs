namespace MrWatchdog.Core.Features.Scrapers.Services;

public record ScrapeResult(
    bool Success, 
    string? Content = null, 
    string? FailureReason = null,
    bool StopWebScraperChain = false
)
{
    public static ScrapeResult Succeeded(string content)
    {
        return new ScrapeResult(Success: true, Content: content);
    }

    public static ScrapeResult Failed(string failureReason, bool stopWebScraperChain = false)
    {
        return new ScrapeResult(
            Success: false,
            FailureReason: failureReason,
            StopWebScraperChain: stopWebScraperChain
        );
    }
}