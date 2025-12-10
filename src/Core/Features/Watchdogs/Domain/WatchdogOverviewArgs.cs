using System.ComponentModel.DataAnnotations;
using MrWatchdog.Core.Infrastructure.Validations;

namespace MrWatchdog.Core.Features.Scrapers.Domain;

public record WatchdogSearchOverviewArgs
{
    [NotDefault]
    public required long WatchdogSearchId { get; set; }

    [StringLength(ValidationConstants.SearchTermMaxLength)]
    public required string? SearchTerm { get; set; }

    public required bool ReceiveNotification { get; set; }
}