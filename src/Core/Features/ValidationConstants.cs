using Reinforced.Typings.Attributes;

namespace MrWatchdog.Core.Features;

[TsClass(IncludeNamespace = false, AutoExportMethods = false)]
public static class ValidationConstants
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const int WatchdogNameMaxLength = 200;

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const int WatchdogWebPageNameMaxLength = 200;

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const int SearchTermMaxLength = 400;

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const int UrlMaxLength = 3000;
}