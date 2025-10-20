using System.ComponentModel.DataAnnotations;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public record PublicWatchdogStatisticsDto
{
    [Display(Name = "Calculated earnings for this month")]
    public required decimal CalculatedEarningsForThisMonth { get; set; }

    [Display(Name = "Number of users with a watchdog search with email notification")]
    public required int NumberOfUsersWithWatchdogSearchWithNotification { get; set; }
    
    [Display(Name = "Number of users with a watchdog search without email notification")]
    public required int NumberOfUsersWithWatchdogSearchWithoutNotification { get; set; }
}