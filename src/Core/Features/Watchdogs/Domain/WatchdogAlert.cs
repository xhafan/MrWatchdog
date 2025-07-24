using CoreDdd.Domain;
using MrWatchdog.Core.Features.Shared.Domain;
using MrWatchdog.Core.Infrastructure.Extensions;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class WatchdogAlert : VersionedEntity, IAggregateRoot
{
    private readonly IList<string> _previousScrapedResults = new List<string>();
    private readonly IList<string> _currentScrapedResults = new List<string>();
    
    protected WatchdogAlert() {}

    public WatchdogAlert(
        Watchdog watchdog,
        string? searchTerm
    )
    {
        Watchdog = watchdog;
        SearchTerm = searchTerm;

        var scrapedResults = watchdog.WebPages.SelectMany(x => x.SelectedElements);
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            scrapedResults = scrapedResults.Where(x => x.Contains(searchTerm)); // todo: extract text from HTML and implement diacritics-insensitive and case-insensitive search
        }
        _currentScrapedResults.AddRange(scrapedResults);
    }
    
    public virtual Watchdog Watchdog { get; protected set; } = null!;
    public virtual string? SearchTerm { get; protected set; }
    public virtual IEnumerable<string> PreviousScrapedResults => _previousScrapedResults;
    public virtual IEnumerable<string> CurrentScrapedResults => _currentScrapedResults;
}