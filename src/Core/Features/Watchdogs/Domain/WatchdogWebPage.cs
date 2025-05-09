using MrWatchdog.Core.Features.Shared.Domain;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class WatchdogWebPage : VersionedEntity
{
    protected WatchdogWebPage() {}

    public virtual string? Name { get; protected set; }
    public virtual string Url { get; protected set; } = null!; // todo: make sure this is a valid url - add regex validation
    public virtual string? Selector { get; protected set; }

    public virtual string GetHtmlSnippet()
    {
        return "";
    }
}