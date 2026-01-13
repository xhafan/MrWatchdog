using System.ComponentModel.DataAnnotations;
using MrWatchdog.Core.Infrastructure.Validations;
using MrWatchdog.Core.Resources;

namespace MrWatchdog.Core.Features.Scrapers.Domain;

public record ScraperOverviewArgs
{
    [NotDefault]
    public required long ScraperId { get; set; }

    [Required]
    [StringLength(ValidationConstants.ScraperNameMaxLength)]
    [Display(Name = nameof(Resource.Name), ResourceType = typeof(Resource))]
    public required string Name { get; set; } = null!;

    [StringLength(ValidationConstants.ScraperDescriptionMaxLength)]
    [Display(Name = nameof(Resource.Description), ResourceType = typeof(Resource))]
    public required string? Description { get; set; }

    [Range(ScrapingConstants.MinimumScrapingIntervalInSeconds, int.MaxValue)]
    [Display(Name = nameof(Resource.ScrapingIntervalInSeconds), ResourceType = typeof(Resource))]
    public required int ScrapingIntervalInSeconds { get; set; }
    
    [Range(0, double.MaxValue)]
    [Display(Name = nameof(Resource.NumberOfDaysBetweenNotificationsForTheSameScrapedResult), ResourceType = typeof(Resource))]
    public required double IntervalBetweenSameResultNotificationsInDays { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = nameof(Resource.NumberOfFailedScrapingAttemptsBeforeAlerting), ResourceType = typeof(Resource))]
    public required int NumberOfFailedScrapingAttemptsBeforeAlerting { get; set; }

}