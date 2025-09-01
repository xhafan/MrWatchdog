namespace MrWatchdog.Core.Messages;

public abstract record BaseMessage
{
    public long ActingUserId { get; set; }
}