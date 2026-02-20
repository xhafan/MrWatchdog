using HtmlAgilityPack;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace MrWatchdog.Core.Infrastructure;

public static partial class HtmlExtractor
{
    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespacesRegex();

    public static string ExtractTextFromHtml(string html)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);
        var textNodes = htmlDoc.DocumentNode
            .DescendantsAndSelf()
            .Where(x => x.NodeType == HtmlNodeType.Text)
            .Select(x => x.InnerText)
            .Where(x => !string.IsNullOrWhiteSpace(x));
        var joinedText = string.Join(" ", textNodes);

        var decoded = HttpUtility.HtmlDecode(joinedText);

        // Normalize Unicode: é vs e + ´(e\u0301)
        var normalized = decoded.Normalize(NormalizationForm.FormC);

        // Normalize whitespace globally
        var normalizedWhitespace = WhitespacesRegex()
            .Replace(normalized, " ")
            .Trim();

        return normalizedWhitespace;
    }

    public static IEnumerable<string> ExtractLinkUrlsFromHtml(string html)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);
        var linkNodes = htmlDoc.DocumentNode.SelectNodes("//a[@href]");
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (linkNodes == null) return [];

        return linkNodes
            .Select(x => x.GetAttributeValue("href", ""))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Where(x => !x.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
                        && !x.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase));
    }
}