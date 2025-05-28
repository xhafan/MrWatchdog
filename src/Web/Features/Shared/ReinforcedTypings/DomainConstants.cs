using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Shared.ReinforcedTypings;

[TsClass(IncludeNamespace = false)]
public static class DomainConstants
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string Watchdog = nameof(Watchdog);
}