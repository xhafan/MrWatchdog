using Reinforced.Typings.Attributes;

namespace MrWatchdog.Core.Features.Watchdogs;

[TsClass(IncludeNamespace = false, AutoExportMethods = false)]
public static class WatchdogUrlConstants
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogIdVariable = "$watchdogId";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string UnsubscribeTokenVariable = "$unsubscribeToken";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogDetailUrlTemplate = $"/Watchdogs/Detail/{WatchdogIdVariable}";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogDetailOverviewUrlTemplate = $"/Watchdogs/Detail/Overview/{WatchdogIdVariable}";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogsUrl = "/Watchdogs";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string DisableWatchdogNotificationsPostUrlTemplate = $"/api/Watchdogs/DisableNotificationPost?unsubscribeToken={UnsubscribeTokenVariable}";

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string DisableWatchdogNotificationsGetUrlTemplate = $"/api/Watchdogs/DisableNotificationGet?unsubscribeToken={UnsubscribeTokenVariable}";

    extension(string urlTemplate)
    {
        public string WithWatchdogId(long watchdogId)
        {
            return urlTemplate.WithVariable(WatchdogIdVariable, watchdogId.ToString());
        }

        public string WithUnsubscribeToken(string unsubscribeToken)
        {
            return urlTemplate.WithVariable(UnsubscribeTokenVariable, unsubscribeToken);
        }
    }
}