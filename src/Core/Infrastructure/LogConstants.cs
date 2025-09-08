using Reinforced.Typings.Attributes;

namespace MrWatchdog.Core.Infrastructure;

[TsClass(IncludeNamespace = false)]
public static class LogConstants
{
    public const string RequestId = nameof(RequestId);
    public const string MessageType = nameof(MessageType);
    public const string MessageId = nameof(MessageId);
    
    [TsProperty(Constant = true, ShouldBeCamelCased = true)]
    public const int MaxLogMessageLength = 2000;
}