namespace MrWatchdog.Core.Features.Scrapers.Domain;

public record ScraperWebPageDisabledWarningDto
{
    public required bool IsEnabled { get; set; }
    public required bool HasBeenScrapedSuccessfully { get; set; }

}