using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MrWatchdog.Web.Features.Shared.ReinforcedTypings;

namespace MrWatchdog.Web.Features.Shared.TagHelpers.TurboFrame;

[HtmlTargetElement("turbo-frame")]
public class TurboFrame(IHtmlHelper htmlHelper) 
    : BaseViewTagHelper(htmlHelper)
{
    public string Id { get; set; } = null!;
    public string Src { get; set; } = null!;
    public string? Loading { get; set; }

    protected override string GetStimulusControllerName()
    {
        return StimulusControllers.TurboFrame;
    }    
}