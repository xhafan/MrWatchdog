using MrWatchdog.Core.Infrastructure.Validations;
using System.ComponentModel.DataAnnotations;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public record WatchdogSearchOverviewArgs
{
    [NotDefault]
    public required long WatchdogSearchId { get; set; }

    [StringLength(ValidationConstants.SearchTermMaxLength)]
    public required string? SearchTerm { get; set; }

    public required bool ReceiveNotification { get; set; }
}