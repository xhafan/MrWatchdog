namespace MrWatchdog.Core.Messages;

public abstract record Command 
{
    public Guid Guid { get; set; } = Guid.NewGuid();
}