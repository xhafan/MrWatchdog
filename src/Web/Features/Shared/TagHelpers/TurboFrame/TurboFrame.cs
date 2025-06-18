using Microsoft.AspNetCore.Razor.TagHelpers;
using MrWatchdog.Web.Features.Shared.ReinforcedTypings;

namespace MrWatchdog.Web.Features.Shared.TagHelpers.TurboFrame;

[HtmlTargetElement("turbo-frame")]
public class TurboFrame : TagHelper
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        await base.ProcessAsync(context, output);

        output.Attributes.SetAttribute("data-controller", StimulusControllers.TurboFrame);
    }
}