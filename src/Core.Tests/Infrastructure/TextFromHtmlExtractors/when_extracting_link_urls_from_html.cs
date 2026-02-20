using MrWatchdog.Core.Infrastructure;

namespace MrWatchdog.Core.Tests.Infrastructure.TextFromHtmlExtractors;

[TestFixture]
public class when_extracting_link_urls_from_html
{
    [Test]
    public void extracted_text_is_correct()
    {
        var html = """
                   <a href="https://www.csfd.cz/film/2294-vykoupeni-z-veznice-shawshank/prehled/" title="Vykoupení z věznice Shawshank">
                       Vykoupení z věznice Shawshank
                   </a>
                   <a href="https://www.csfd.cz/film/10135-forrest-gump/prehled/" title="Forrest Gump">
                      	Forrest Gump
                   </a>
                   <a href="mailto:support@mrwatchdog.com" class="fw-semibold text-decoration-none">support@mrwatchdog.com</a>
                   <a href="javascript:void(0);">JS link</a>
                   """;

        HtmlExtractor.ExtractLinkUrlsFromHtml(html).ShouldBe([
            "https://www.csfd.cz/film/2294-vykoupeni-z-veznice-shawshank/prehled/",
            "https://www.csfd.cz/film/10135-forrest-gump/prehled/"
        ]);
    }
}