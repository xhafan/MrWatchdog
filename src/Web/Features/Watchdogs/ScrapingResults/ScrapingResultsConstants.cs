using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Watchdogs.ScrapingResults;

[TsClass(IncludeNamespace = false)]
public static class ScrapingResultsConstants
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string CreateAlertButtonNewResultsLabel = "Alert about new results";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string SearchTermVariable = "$searchTerm";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string CreateAlertButtonNewMatchingResultsLabelTemplate = $"Alert about new results matching search term <i>{SearchTermVariable}</i>";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string LoginOrRegisterToCreateAlertButtonNewResultsLabel = "Log in or register to alert about new results";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string LoginOrRegisterToCreateAlertButtonNewMatchingResultsLabelTemplate = 
        $"Log in or register to alert about new results matching search term <i>{SearchTermVariable}</i>";
}