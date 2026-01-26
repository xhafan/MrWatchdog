using System.ComponentModel.DataAnnotations;
using MrWatchdog.Core.Resources;

namespace MrWatchdog.Core.Features.Scrapers.Domain;

public enum ScrapingByBrowserWaitFor
{
    [Display(Name = nameof(Resource.ScrapingByBrowserWaitForDomContentLoaded), ResourceType = typeof(Resource))]
    DomContentLoaded = 'D',
    
    [Display(Name = nameof(Resource.ScrapingByBrowserWaitForLoad), ResourceType = typeof(Resource))]
    Load = 'L',
    
    [Display(Name = nameof(Resource.ScrapingByBrowserWaitForNetworkIdle), ResourceType = typeof(Resource))]
    NetworkIdle = 'N'
}