using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Watchdogs.Detail;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public record DetailStimulusModel(
    string? GetJobUrl, 
    string? WebPageToMonitorTurboFrameUrl
)
{
    public string GetJobUrl { get; init; } = GetJobUrl ?? throw new ArgumentNullException(nameof(GetJobUrl));
    public string WebPageToMonitorTurboFrameUrl { get; init; } = WebPageToMonitorTurboFrameUrl ?? throw new ArgumentNullException(nameof(WebPageToMonitorTurboFrameUrl));
}
