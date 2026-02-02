using Reinforced.Typings.Attributes;

namespace MrWatchdog.Core.Features.Watchdogs;

[TsClass(IncludeNamespace = false, AutoExportMethods = false)]
public static class WatchdogUrlConstants
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogIdVariable = "$watchdogId";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogDetailUrlTemplate = $"/Watchdogs/Detail/{WatchdogIdVariable}";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogDetailOverviewUrlTemplate = $"/Watchdogs/Detail/Overview/{WatchdogIdVariable}";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogsUrl = "/Watchdogs";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string DisableWatchdogNotificationsUrlTemplate = $"/api/Watchdogs/{WatchdogIdVariable}/DisableNotification";

    extension(string urlTemplate)
    {
        public string WithWatchdogId(long watchdogId)
        {
            return urlTemplate.WithVariable(WatchdogIdVariable, watchdogId.ToString());
        }
    }
}