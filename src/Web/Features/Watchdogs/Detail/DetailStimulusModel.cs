using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Watchdogs.Detail;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public record DetailStimulusModel(string? GetJobUrl, string? WebPageToMonitorUrl)
{
    public string GetJobUrl { get; init; } = GetJobUrl ?? throw new ArgumentNullException(nameof(GetJobUrl));
    public string WebPageToMonitorUrl { get; init; } = WebPageToMonitorUrl ?? throw new ArgumentNullException(nameof(WebPageToMonitorUrl));
}
