using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Watchdogs.Detail;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public record DetailStimulusModel(
    string? WebPageTurboFrameUrl
)
{
    public string WebPageTurboFrameUrl { get; init; } = WebPageTurboFrameUrl ?? throw new ArgumentNullException(nameof(WebPageTurboFrameUrl));
}
