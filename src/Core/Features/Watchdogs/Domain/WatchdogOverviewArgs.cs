using System.ComponentModel.DataAnnotations;
using CoreBackend.Infrastructure.Validations;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public record WatchdogOverviewArgs
{
    [NotDefault]
    public required long WatchdogId { get; set; }

    [StringLength(ValidationConstants.SearchTermMaxLength)]
    public required string? SearchTerm { get; set; }

    public required bool ReceiveNotification { get; set; }
}