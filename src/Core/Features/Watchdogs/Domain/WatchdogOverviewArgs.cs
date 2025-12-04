using MrWatchdog.Core.Infrastructure.Validations;
using MrWatchdog.Core.Resources;
using System.ComponentModel.DataAnnotations;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public record WatchdogOverviewArgs
{
    [NotDefault]
    public required long WatchdogId { get; set; }

    [Required]
    [StringLength(ValidationConstants.WatchdogNameMaxLength)]
    [Display(Name = nameof(Resource.Name), ResourceType = typeof(Resource))]
    public required string Name { get; set; } = null!;

    [StringLength(ValidationConstants.WatchdogDescriptionMaxLength)]
    [Display(Name = nameof(Resource.Description), ResourceType = typeof(Resource))]
    public required string? Description { get; set; }

    [Range(ScrapingConstants.ScrapingIntervalInSeconds, int.MaxValue)]
    [Display(Name = nameof(Resource.ScrapingIntervalInSeconds), ResourceType = typeof(Resource))]
    public required int ScrapingIntervalInSeconds { get; set; }
    
    [Range(0, double.MaxValue)]
    [Display(Name = nameof(Resource.NumberOfDaysBetweenNotificationsForTheSameScrapedResult), ResourceType = typeof(Resource))]
    public required double IntervalBetweenSameResultNotificationsInDays { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = nameof(Resource.NumberOfFailedScrapingAttemptsBeforeAlerting), ResourceType = typeof(Resource))]
    public required int NumberOfFailedScrapingAttemptsBeforeAlerting { get; set; }

}