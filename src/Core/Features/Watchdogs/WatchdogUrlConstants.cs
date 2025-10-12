using Reinforced.Typings.Attributes;

namespace MrWatchdog.Core.Features.Watchdogs;

[TsClass(IncludeNamespace = false, AutoExportMethods = false)]
public static class WatchdogUrlConstants
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]    
    public const string WatchdogIdVariable = "$watchdogId";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogWebPageIdVariable = "$watchdogWebPageId";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogSearchIdVariable = "$watchdogSearchId";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogsUrl = "/Watchdogs";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WachdogsPublicWatchdogsUrl = "/Watchdogs/PublicWatchdogs";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogsUserWatchdogsUrl = "/Watchdogs/UserWatchdogs";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogScrapingResultsUrlTemplate = $"/Watchdogs/ScrapingResults/{WatchdogIdVariable}";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogCreateUrl = "/Watchdogs/Create";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogDetailUrlTemplate = $"/Watchdogs/Detail/{WatchdogIdVariable}";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogDetailOverviewUrlTemplate = $"/Watchdogs/Detail/Overview/{WatchdogIdVariable}";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogDetailWebPageUrlTemplate = 
        $"/Watchdogs/Detail/WebPage?watchdogId={WatchdogIdVariable}&watchdogWebPageId={WatchdogWebPageIdVariable}";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogDetailWebPageOverviewUrlTemplate = 
        $"/Watchdogs/Detail/WebPage/WebPageOverview?watchdogId={WatchdogIdVariable}&watchdogWebPageId={WatchdogWebPageIdVariable}";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogDetailWebPageTurboFrameUrlTemplate = 
        $"/Watchdogs/Detail/WebPage/WebPageTurboFrame?watchdogId={WatchdogIdVariable}&watchdogWebPageId={WatchdogWebPageIdVariable}";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogDetailWebPageScrapingResultsUrlTemplate = 
        $"/Watchdogs/Detail/WebPage/WebPageScrapingResults?watchdogId={WatchdogIdVariable}&watchdogWebPageId={WatchdogWebPageIdVariable}";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogDetailBadgesUrlTemplate = $"/Watchdogs/Detail/Badges/{WatchdogIdVariable}";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogDetailActionsUrlTemplate = $"/Watchdogs/Detail/Actions/{WatchdogIdVariable}";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogDetailActionsMakePublicUrlTemplate = $"/Watchdogs/Detail/Actions/{WatchdogIdVariable}?handler=MakePublic";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogSearchUrlTemplate = $"/Watchdogs/Search/{WatchdogSearchIdVariable}";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogSearchOverviewUrlTemplate = $"/Watchdogs/Search/Overview/{WatchdogSearchIdVariable}";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogsManageUrl = "/Watchdogs/Manage";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogsManageUserWatchdogsUrl = "/Watchdogs/Manage/UserWatchdogs";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogsManageOtherUsersWatchdogsUrl = "/Watchdogs/Manage/OtherUsersWatchdogs";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogsSearchesUrl = "/Watchdogs/Searches";

    
    public static string WithWatchdogId(this string urlTemplate, long watchdogId)
    {
        return urlTemplate.WithVariable(WatchdogIdVariable, watchdogId.ToString());
    }

    public static string WithWatchdogWebPageIdVariable(this string urlTemplate, long watchdogWebPageId)
    {
        return urlTemplate.WithVariable(WatchdogWebPageIdVariable, watchdogWebPageId.ToString());
    }

    public static string WithWatchdogSearchId(this string urlTemplate, long watchdogSearchId)
    {
        return urlTemplate.WithVariable(WatchdogSearchIdVariable, watchdogSearchId.ToString());
    }
}