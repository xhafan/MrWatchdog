using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Web.Features.Shared.TagHelpers;

namespace MrWatchdog.Web.Features.Watchdogs.Shared.PublicStatusBadge;

[HtmlTargetElement("public-status-badge")]
public class PublicStatusBadge(IHtmlHelper htmlHelper) : BaseViewTagHelper(htmlHelper)
{
    protected override string TagName => "span";
    
    public PublicStatus PublicStatus { get; set; }
    public bool ShowPrivate { get; set; } = true;
}