﻿using Reinforced.Typings.Attributes;

namespace MrWatchdog.Core.Infrastructure.Rebus;

[TsClass(IncludeNamespace = false)]
public static class RebusConstants
{
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const int MaxDeliveryAttempts = 5;
}