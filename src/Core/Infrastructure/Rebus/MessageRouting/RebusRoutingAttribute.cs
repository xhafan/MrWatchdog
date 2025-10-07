namespace MrWatchdog.Core.Infrastructure.Rebus.MessageRouting;

[AttributeUsage(AttributeTargets.Class)]
public class RebusRoutingAttribute(string queue) : Attribute
{
    public string Queue { get; } = queue;
}