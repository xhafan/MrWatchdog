using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Watchdogs.Create;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public record CreateStimulusModel(string? GetJobUrl, string? WatchdogDetailUrl)
{
    public string GetJobUrl { get; init; } = GetJobUrl ?? throw new ArgumentNullException(nameof(GetJobUrl));
    public string WatchdogDetailUrl { get; init; } = WatchdogDetailUrl ?? throw new ArgumentNullException(nameof(WatchdogDetailUrl));
}
