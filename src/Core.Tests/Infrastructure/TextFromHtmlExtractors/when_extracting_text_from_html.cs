using MrWatchdog.Core.Infrastructure;

namespace MrWatchdog.Core.Tests.Infrastructure.TextFromHtmlExtractors;

[TestFixture]
public class when_extracting_text_from_html
{
    [TestCase(
        """
        <a href="https://www.csfd.cz/film/2294-vykoupeni-z-veznice-shawshank/prehled/" title="Vykoupení z věznice Shawshank">
            Vykoupení z věznice Shawshank
        </a>
        """,
        "Vykoupení z věznice Shawshank", TestName = "1 valid html")]
    [TestCase(
        """
        a href="https://www.csfd.cz/film/2294-vykoupeni-z-veznice-shawshank/prehled/" title="Vykoupení z věznice Shawshank">
            Vykoupení z věznice Shawshank
        </a>
        """,
        """
        a href="https://www.csfd.cz/film/2294-vykoupeni-z-veznice-shawshank/prehled/" title="Vykoupení z věznice Shawshank"> Vykoupení z věznice Shawshank
        """, TestName = "2 invalid html")]
    [TestCase(
        """
          just some text with spaces around
        """,
        """
        just some text with spaces around
        """, TestName = "3 plain text")]
    public void extracted_text_is_correct(string html, string expectedText)
    {
        HtmlExtractor.ExtractTextFromHtml(html).ShouldBe(expectedText);
    }
}