using System.ComponentModel.DataAnnotations;
using MrWatchdog.Core.Infrastructure.Validations;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public record WatchdogOverviewArgs
{
    [NotDefault]
    public long WatchdogId { get; set; }

    [Required]
    public string Name { get; set; } = null!;
}