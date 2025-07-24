using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Watchdogs;

[TsClass(IncludeNamespace = false)]
public static class WatchdogConstants
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]    
    public const string WatchdogIdVariable = "$watchdogId";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogDetailUrl = $"/Watchdogs/Detail/{WatchdogIdVariable}";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogWebPageIdVariable = "$watchdogWebPageId";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogDetailWebPageTurboFrameUrl = 
        $"/Watchdogs/Detail/WebPage/WebPageTurboFrame?watchdogId={WatchdogIdVariable}&watchdogWebPageId={WatchdogWebPageIdVariable}";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogAlertIdVariable = "$watchdogAlertId";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogAlertUrl = $"/Watchdogs/Alert/{WatchdogAlertIdVariable}";
}