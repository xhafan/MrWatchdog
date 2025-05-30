using System.ComponentModel.DataAnnotations;
using MrWatchdog.Core.Infrastructure.Validations;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class WatchdogOverviewArgs
{
    [NotDefault]
    public long Id { get; set; }

    [Required]
    public string Name { get; set; } = null!;
}