using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Shared;

[TsClass(IncludeNamespace = false)]
public static class DomEvents
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScraperPublicStatusUpdated = "scraperPublicStatusUpdated";
}