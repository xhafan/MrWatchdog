using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Shared.TagHelpers;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public abstract record BaseStimulusModel
{
    public string GetJobUrl { get; set; } = null!;
}