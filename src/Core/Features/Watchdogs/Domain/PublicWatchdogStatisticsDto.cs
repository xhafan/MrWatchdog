using System.ComponentModel.DataAnnotations;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public record PublicWatchdogStatisticsDto // todo: rename to PublicWebScraperStatisticsDto
{
    [Display(Name = "Calculated earnings for this month")]
    public required decimal CalculatedEarningsForThisMonth { get; set; }

    [Display(Name = "Number of users with a watchdog with email notification")]
    public required int NumberOfUsersWithWatchdogWithNotification { get; set; }
    
    [Display(Name = "Number of users with a watchdog without email notification")]
    public required int NumberOfUsersWithWatchdogWithoutNotification { get; set; }
}