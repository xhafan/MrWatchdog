using MrWatchdog.Core.Resources;
using System.ComponentModel.DataAnnotations;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public record PublicWatchdogStatisticsDto // todo: rename to PublicWebScraperStatisticsDto
{
    [Display(Name = nameof(Resource.CalculatedEarningsForThisMonth), ResourceType = typeof(Resource))]
    public required decimal CalculatedEarningsForThisMonth { get; set; }

    [Display(Name = nameof(Resource.NumberOfUsersWithWatchdogWithEmailNotification), ResourceType = typeof(Resource))]
    public required int NumberOfUsersWithWatchdogWithNotification { get; set; }
    
    [Display(Name = nameof(Resource.NumberOfUsersWithWatchdogWithoutEmailNotification), ResourceType = typeof(Resource))]
    public required int NumberOfUsersWithWatchdogWithoutNotification { get; set; }
}