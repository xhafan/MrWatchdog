using Reinforced.Typings.Attributes;

namespace MrWatchdog.Core.Features.Scrapers;

[TsClass(IncludeNamespace = false, AutoExportMethods = false)]
public static class ScraperUrlConstants // todo: move WatchdogSearch to WatchdogUrlConstants
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]    
    public const string ScraperIdVariable = "$scraperId";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScraperWebPageIdVariable = "$scraperWebPageId";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogSearchIdVariable = "$watchdogSearchId";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScrapersUrl = "/Scrapers";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScrapersPublicScrapersUrl = "/Scrapers/PublicScrapers";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScrapersUserScrapersUrl = "/Scrapers/UserScrapers";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScraperScrapingResultsUrlTemplate = $"/Scrapers/ScrapingResults/{ScraperIdVariable}";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScraperCreateUrl = "/Scrapers/Create";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScraperDetailUrlTemplate = $"/Scrapers/Detail/{ScraperIdVariable}";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScraperDetailOverviewUrlTemplate = $"/Scrapers/Detail/Overview/{ScraperIdVariable}";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScraperDetailStatisticsUrlTemplate = $"/Scrapers/Detail/Statistics/{ScraperIdVariable}";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScraperDetailWebPageUrlTemplate = 
        $"/Scrapers/Detail/WebPage?scraperId={ScraperIdVariable}&scraperWebPageId={ScraperWebPageIdVariable}";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScraperDetailWebPageOverviewUrlTemplate = 
        $"/Scrapers/Detail/WebPage/WebPageOverview?scraperId={ScraperIdVariable}&scraperWebPageId={ScraperWebPageIdVariable}";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScraperDetailWebPageTurboFrameUrlTemplate = 
        $"/Scrapers/Detail/WebPage/WebPageTurboFrame?scraperId={ScraperIdVariable}&scraperWebPageId={ScraperWebPageIdVariable}";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScraperDetailWebPageScrapingResultsUrlTemplate = 
        $"/Scrapers/Detail/WebPage/WebPageScrapingResults?scraperId={ScraperIdVariable}&scraperWebPageId={ScraperWebPageIdVariable}";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScraperDetailBadgesUrlTemplate = $"/Scrapers/Detail/Badges/{ScraperIdVariable}";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScraperDetailActionsUrlTemplate = $"/Scrapers/Detail/Actions/{ScraperIdVariable}";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScraperDetailActionsMakePublicUrlTemplate = $"/Scrapers/Detail/Actions/{ScraperIdVariable}?handler=MakePublic";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogSearchUrlTemplate = $"/Scrapers/Search/{WatchdogSearchIdVariable}";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogSearchOverviewUrlTemplate = $"/Scrapers/Search/Overview/{WatchdogSearchIdVariable}";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScrapersManageUrl = "/Scrapers/Manage";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScrapersManageUserScrapersUrl = "/Scrapers/Manage/UserScrapers";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ScrapersManageOtherUsersScrapersUrl = "/Scrapers/Manage/OtherUsersScrapers";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogsSearchesUrl = "/Scrapers/Searches";

    
    extension(string urlTemplate)
    {
        public string WithScraperId(long scraperId)
        {
            return urlTemplate.WithVariable(ScraperIdVariable, scraperId.ToString());
        }

        public string WithScraperWebPageIdVariable(long scraperWebPageId)
        {
            return urlTemplate.WithVariable(ScraperWebPageIdVariable, scraperWebPageId.ToString());
        }

        public string WithWatchdogSearchId(long watchdogSearchId)
        {
            return urlTemplate.WithVariable(WatchdogSearchIdVariable, watchdogSearchId.ToString());
        }
    }
}