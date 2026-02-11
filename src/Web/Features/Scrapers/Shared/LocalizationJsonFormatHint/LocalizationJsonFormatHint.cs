using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MrWatchdog.Web.Features.Shared.TagHelpers;

namespace MrWatchdog.Web.Features.Scrapers.Shared.LocalizationJsonFormatHint;

[HtmlTargetElement("localization-json-format-hint")]
public class LocalizationJsonFormatHint(IHtmlHelper htmlHelper) : BaseViewTagHelper(htmlHelper)
{
    protected override string TagName => "";
}