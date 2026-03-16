namespace MrWatchdog.Core.Messages;

public abstract record Command : BaseMessage
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public Guid Guid { get; set; } // public setter is needed when Rebus deserializes the base message
}