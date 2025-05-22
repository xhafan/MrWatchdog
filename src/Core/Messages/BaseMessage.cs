namespace MrWatchdog.Core.Messages;

public abstract record BaseMessage
{
    public Guid Guid { get; set; } = Guid.NewGuid(); // public setter is needed when Rebus deserializes the base message
}