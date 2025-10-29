using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Watchdogs.ScrapingResults;

[TsClass(IncludeNamespace = false)]
public static class ScrapingResultsConstants
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string SaveSearchAndNotifyMeAboutNewResultsButtonLabel = "Save Search with email notification about new search results";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string SearchTermVariable = "$searchTerm";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string SaveSearchAndNotifyMeAboutNewMatchingResultsButtonLabelTemplate 
        = $"""
           Save Search with email notification about new search results matching the search term <i translate="no">{SearchTermVariable}</i>
           """;
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string LoginOrRegisterToSaveSearchAndNotifyMeAboutNewResultsButtonLabel 
        = "Log in or register to save Search with email notification about new search results";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string LoginOrRegisterToSaveSearchAndNotifyMeAboutNewMatchingResultsButtonLabelTemplate = 
        $"""
         Log in or register to save Search with email notification about new search results matching the search term <i translate="no">{SearchTermVariable}</i>
         """;
}