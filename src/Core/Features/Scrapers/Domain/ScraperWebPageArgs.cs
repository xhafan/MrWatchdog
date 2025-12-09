using System.ComponentModel.DataAnnotations;
using MrWatchdog.Core.Infrastructure.Validations;
using MrWatchdog.Core.Resources;

namespace MrWatchdog.Core.Features.Scrapers.Domain;

public record ScraperWebPageArgs
{
    [NotDefault]
    public long ScraperId { get; set; }
    [NotDefault]
    public long ScraperWebPageId { get; set; }

    [Url]
    [Required]
    [StringLength(ValidationConstants.UrlMaxLength)]
    [Display(Name = nameof(Resource.Url), ResourceType = typeof(Resource))]
    public string? Url { get; set; }

    [Display(Name = nameof(Resource.Selector), ResourceType = typeof(Resource))]
    [StringLength(1500)]
    [Required]
    public string? Selector { get; set; }
    
    [Display(Name = nameof(Resource.SelectTextInsteadOfHtml), ResourceType = typeof(Resource))]
    public bool SelectText { get; set; }
    
    [Required]
    [StringLength(ValidationConstants.ScraperWebPageNameMaxLength)]
    [Display(Name = nameof(Resource.Name), ResourceType = typeof(Resource))]
    public string? Name { get; set; }

    [Display(Name = nameof(Resource.HttpHeaders), ResourceType = typeof(Resource))]
    [StringLength(3000)]
    public string? HttpHeaders { get; set; }
}