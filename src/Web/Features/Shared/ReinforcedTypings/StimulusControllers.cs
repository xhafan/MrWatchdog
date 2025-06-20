﻿using Reinforced.Typings.Attributes;

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
}