using CoreWeb.Features.Shared.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace MrWatchdog.Web.Features.Scrapers.Shared.LocalizationJsonFormatHint;

[HtmlTargetElement("localization-json-format-hint")]
public class LocalizationJsonFormatHint(IHtmlHelper htmlHelper) : BaseViewTagHelper(htmlHelper)
{
    protected override string TagName => "";
}