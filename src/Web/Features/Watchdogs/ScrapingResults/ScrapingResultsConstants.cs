using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Watchdogs.ScrapingResults;

[TsClass(IncludeNamespace = false)]
public static class ScrapingResultsConstants
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string SaveSearchAndNotifyMeAboutNewResultsButtonLabel = "Save Search with notification about new results";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string SearchTermVariable = "$searchTerm";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string SaveSearchAndNotifyMeAboutNewMatchingResultsButtonLabelTemplate 
        = $"Save Search with notification about new results matching the search term <i>{SearchTermVariable}</i>";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string LoginOrRegisterToSaveSearchAndNotifyMeAboutNewResultsButtonLabel = "Log in or register to save Search with notification about new results";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string LoginOrRegisterToSaveSearchAndNotifyMeAboutNewMatchingResultsButtonLabelTemplate = 
        $"Log in or register to save Search with notification about new results matching the search term <i>{SearchTermVariable}</i>";
}