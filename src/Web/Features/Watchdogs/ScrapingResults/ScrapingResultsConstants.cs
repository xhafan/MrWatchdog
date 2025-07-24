using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Watchdogs.ScrapingResults;

[TsClass(IncludeNamespace = false)]
public static class ScrapingResultsConstants
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string CreateAlertButtonDefaultLabel = "Alert about new results";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string CreateAlertButtonSearchLabel = "Alert about new matching results";
}