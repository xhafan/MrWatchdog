using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Shared.ReinforcedTypings;

[TsClass(IncludeNamespace = false)]
public static class StimulusControllers
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string Body = "body";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string TurboFrame = "turbo-frame";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string ViewOrEditForm = "view-or-edit-form";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogsCreate = "watchdogs--create";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogsDetail = "watchdogs--detail";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogsDetailWebPage = "watchdogs--detail-web-page";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogsDetailWebPageOverview = "watchdogs--detail-web-page-overview";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogsScrapingResults = "watchdogs--scraping-results";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogsScrapingResultsWebPages = "watchdogs--scraping-results-web-pages";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogAlert = "watchdogs--alert";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogAlertOverview = "watchdogs--alert-overview";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string AccountLogin = "account--login";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string AccountLoginLinkSent = "account--login-link-sent";
}