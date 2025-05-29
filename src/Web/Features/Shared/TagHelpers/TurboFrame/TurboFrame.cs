using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MrWatchdog.Web.Features.Shared.TagHelpers.TurboFrame;

[HtmlTargetElement("turbo-frame")]
public class TurboFrame(IHtmlHelper htmlHelper, LinkGenerator linkGenerator) 
    : BaseStimulusModelViewTagHelper<TurboFrameStimulusModel>(htmlHelper, linkGenerator)
{
    public string Id { get; set; } = null!;
    public string? Src { get; set; }
    public string? Loading { get; set; }
    public string? ReloadOnEvent { get; set; }

    protected override string GetStimulusControllerName()
    {
        return StimulusControllers.TurboFrame;
    }    
    
    protected override TurboFrameStimulusModel GetStimulusModel()
    {
        return new TurboFrameStimulusModel {ReloadOnEvent = ReloadOnEvent};
    }
}