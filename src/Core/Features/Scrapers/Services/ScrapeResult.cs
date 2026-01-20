namespace MrWatchdog.Core.Features.Scrapers.Services;

public record ScrapeResult
{
    private ScrapeResult(
        bool Success, 
        string? Content = null, 
        string? FailureReason = null,
        bool StopWebScraperChain = false,
        int HttpStatusCode = 0
    )
    {
        this.Success = Success;
        this.Content = Content;
        this.FailureReason = FailureReason;
        this.StopWebScraperChain = StopWebScraperChain;
        this.HttpStatusCode = HttpStatusCode;
    }

    public static ScrapeResult Succeeded(
        string content,
        int httpStatusCode
    )
    {
        return new ScrapeResult(
            Success: true,
            Content: content,
            HttpStatusCode: httpStatusCode
        );
    }

    public static ScrapeResult Failed(
        string failureReason, 
        bool stopWebScraperChain = false,
        int httpStatusCode = 0
    )
    {
        return new ScrapeResult(
            Success: false,
            FailureReason: failureReason,
            StopWebScraperChain: stopWebScraperChain,
            HttpStatusCode: httpStatusCode
        );
    }

    public bool Success { get; }
    public string? Content { get; }
    public string? FailureReason { get; }
    public bool StopWebScraperChain { get; }
    public int HttpStatusCode { get; }
}