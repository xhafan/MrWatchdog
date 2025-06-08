using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Watchdogs.Create;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public record CreateStimulusModel(string? WatchdogDetailUrl)
{
    public string WatchdogDetailUrl { get; init; } = WatchdogDetailUrl ?? throw new ArgumentNullException(nameof(WatchdogDetailUrl));
}
