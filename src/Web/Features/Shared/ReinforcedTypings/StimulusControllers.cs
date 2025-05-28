using Reinforced.Typings.Attributes;

namespace MrWatchdog.Web.Features.Shared.ReinforcedTypings;

[TsClass(IncludeNamespace = false)]
public static class StimulusControllers
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogsCreate = "watchdogs--create";
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const string WatchdogsDetail = "watchdogs--detail";
    
}