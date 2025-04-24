using MrWatchdog.Core.Features.Shared.Domain;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class WatchdogWeb : VersionedEntity
{
    protected WatchdogWeb() {}

    public virtual string Url { get; protected set; } = null!; // todo: make sure this is a valid url - add regex validation
    public virtual string Selector { get; protected set; } = null!;

    public virtual string GetHtmlSnippet()
    {
        return "";
    }
}