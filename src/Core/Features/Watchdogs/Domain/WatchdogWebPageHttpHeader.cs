namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public record WatchdogWebPageHttpHeader
{
    protected WatchdogWebPageHttpHeader() {}

    public WatchdogWebPageHttpHeader(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; protected set; } = null!;
    public string Value { get; protected set; } = null!;
}