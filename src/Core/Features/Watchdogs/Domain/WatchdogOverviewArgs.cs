using System.ComponentModel.DataAnnotations;
using MrWatchdog.Core.Infrastructure.Validations;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public record WatchdogOverviewArgs
{
    [NotDefault]
    public required long WatchdogId { get; set; }

    [Required]
    public required string Name { get; set; } = null!;

    [Range(ScrapingConstants.ScrapingIntervalInSeconds, int.MaxValue, ErrorMessage = "The {0} must be greater than or equal to 10.")]
    [Display(Name = "Scraping interval in seconds")]
    public required int ScrapingIntervalInSeconds { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "The {0} must be greater than or equal to 0.")]
    [Display(Name = "Number of days between alerts for the same result")]
    public required double IntervalBetweenSameResultAlertsInDays { get; set; }
}