using MrWatchdog.Core.Infrastructure.Validations;
using MrWatchdog.Core.Resources;
using System.ComponentModel.DataAnnotations;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public record WatchdogWebPageArgs
{
    [NotDefault]
    public long WatchdogId { get; set; }
    [NotDefault]
    public long WatchdogWebPageId { get; set; }

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
    [StringLength(ValidationConstants.WatchdogWebPageNameMaxLength)]
    [Display(Name = nameof(Resource.Name), ResourceType = typeof(Resource))]
    public string? Name { get; set; }

    [Display(Name = nameof(Resource.HttpHeaders), ResourceType = typeof(Resource))]
    [StringLength(3000)]
    public string? HttpHeaders { get; set; }
}