namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public record WatchdogWebPageDisabledWarningDto
{
    public required bool IsEnabled { get; set; }
    public required bool HasBeenScrapedSuccessfully { get; set; }

}