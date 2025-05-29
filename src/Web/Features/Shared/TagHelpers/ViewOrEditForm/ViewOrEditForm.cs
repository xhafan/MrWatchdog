using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MrWatchdog.Web.Features.Shared.TagHelpers.ViewOrEditForm;

[HtmlTargetElement("view-or-edit-form")]
public class ViewOrEditForm(IHtmlHelper htmlHelper, LinkGenerator linkGenerator) 
    : BaseStimulusModelViewTagHelper<ViewOrEditFormStimulusModel>(htmlHelper, linkGenerator)
{
    public string? Action { get; set; }
    
    protected override string GetStimulusControllerName()
    {
        return StimulusControllers.ViewOrEditForm;
    }

    protected override ViewOrEditFormStimulusModel GetStimulusModel()
    {
        return new ViewOrEditFormStimulusModel();
    }
}