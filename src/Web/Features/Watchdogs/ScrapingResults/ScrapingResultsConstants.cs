using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Watchdogs.ScrapingResults;

[TsClass(IncludeNamespace = false)]
public static class ScrapingResultsConstants
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string CreateAlertButtonNewResultsLabel = "Alert about new results";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string CreateAlertButtonNewMatchingResultsLabel = "Alert about new matching results";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string LoginOrRegisterToCreateAlertButtonNewResultsLabel = "Log in or register to alert about new results";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string LoginOrRegisterToCreateAlertButtonNewMatchingResultsLabels = "Log in or register to alert about new matching results";
    
}