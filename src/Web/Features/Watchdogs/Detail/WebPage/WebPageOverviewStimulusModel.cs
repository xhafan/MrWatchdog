using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public record WebPageOverviewStimulusModel(bool IsEmptyWebPage);
