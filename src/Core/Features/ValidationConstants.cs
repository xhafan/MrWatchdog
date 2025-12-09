using Reinforced.Typings.Attributes;

namespace MrWatchdog.Core.Features;

[TsClass(IncludeNamespace = false, AutoExportMethods = false)]
public static class ValidationConstants
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const int ScraperNameMaxLength = 200;

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const int ScraperDescriptionMaxLength = 1000;

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const int ScraperWebPageNameMaxLength = 200;

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const int SearchTermMaxLength = 400;

    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const int UrlMaxLength = 3000;
}