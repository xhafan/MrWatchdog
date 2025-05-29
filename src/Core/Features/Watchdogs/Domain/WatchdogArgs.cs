using MrWatchdog.Core.Infrastructure.Validations;
using System.ComponentModel.DataAnnotations;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class WatchdogArgs
{
    [NotDefault]
    public long Id { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    public IList<long> WebPageIds { get; set; } = new List<long>();
}
