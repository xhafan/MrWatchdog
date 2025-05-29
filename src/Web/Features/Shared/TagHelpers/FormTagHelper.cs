using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MrWatchdog.Web.Features.Shared.TagHelpers;

public class FormTagHelper(IHtmlGenerator htmlGenerator) : Microsoft.AspNetCore.Mvc.TagHelpers.FormTagHelper(htmlGenerator)
{
    public bool PreventImplicitSubmit { get; set; }
    
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        await base.ProcessAsync(context, output);

        if (PreventImplicitSubmit)
        {
            var disabledSubmitButtonToPreventImplicitFormSubmission =
                """
                <button type="submit" disabled style="display: none"></button>
                """;

            output.PreContent.AppendHtml(disabledSubmitButtonToPreventImplicitFormSubmission);
        }
    }
}