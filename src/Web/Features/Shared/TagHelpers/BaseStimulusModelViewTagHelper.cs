using CoreUtils;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MrWatchdog.Core.Infrastructure;

namespace MrWatchdog.Web.Features.Shared.TagHelpers;

public abstract class BaseStimulusModelViewTagHelper<TStimulusModel>(IHtmlHelper htmlHelper, LinkGenerator linkGenerator) 
    : BaseViewTagHelper(htmlHelper) where TStimulusModel : BaseStimulusModel
{
    protected abstract TStimulusModel GetStimulusModel();
    
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        await base.ProcessAsync(context, output);

        var stimulusModel = GetStimulusModel();
        var getJobUrl = linkGenerator.GetPathByAction("GetJob", "Jobs", new { jobGuid = "$jobGuid"});
        Guard.Hope(getJobUrl != null, nameof(getJobUrl) + " is null");
        stimulusModel.GetJobUrl = getJobUrl;
        
        var stimulusControllerName = GetStimulusControllerName();
        Guard.Hope(stimulusControllerName != null, $"{GetType().Name} Stimulus controller name not provided.");
        output.Attributes.SetAttribute($"data-{stimulusControllerName}-model-value", JsonHelper.Serialize(stimulusModel));
    }
}