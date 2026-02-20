using MrWatchdog.Core.Features.Scrapers.Domain;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain;

[TestFixture]
public class when_comparing_scraped_results_equality
{
    [TestCase("<div>téxt1 text2</div>", "<div> te\u0301xt1  text2 </div>", true, TestName = "1 the same text with some extra whitespace")]
    [TestCase("<div>text</div>", "<div> text2 </div>", false, TestName = "2 different text")]
    [TestCase(
        """
        <div>text1 text2<a href="http://url.com">link</a></div>
        """, 
        """
        <div> text1  text2 <a href="http://URL.com"> link </a></div>
        """, true, TestName = "3 the same text and the same url links with some extra whitespace")]
    [TestCase(
        """
        <div>text<a href="http://url.com">link</a></div>
        """, 
        """
        <div>text<a href="http://url2.com">link</a></div>
        """, false, TestName = "4 the same text and different url links")]
    public void comparison_is_correct(string htmlOne, string htmlTwo, bool expectedAreEqual)
    {
        var scrapedResultOne = new ScrapedResult(htmlOne);
        var scrapedResultTwo = new ScrapedResult(htmlTwo);

        scrapedResultOne.Equals(scrapedResultTwo).ShouldBe(expectedAreEqual);
    }

    [Test]
    public void adding_scrape_results_to_hashset()
    {
        var hashSet = new HashSet<ScrapedResult>
        {
            new("<div>text1 text2</div>"),
            new("<div>  text1  text2  </div>")
        };

        hashSet.Count.ShouldBe(1);
    }

    [Test]
    public void equal_operator_for_equal()
    {
        (new ScrapedResult("<div>text1 text2</div>") == new ScrapedResult("<div>  text1  text2  </div>")).ShouldBe(true);
    }

    [Test]
    public void equal_operator_for_not_equal()
    {
        (new ScrapedResult("<div>text1 text2</div>") == new ScrapedResult("<div>  text1  text3  </div>")).ShouldBe(false);
    }

    [Test]
    public void not_equal_operator_for_equal()
    {
        (new ScrapedResult("<div>text1 text2</div>") != new ScrapedResult("<div>  text1  text2  </div>")).ShouldBe(false);
    }

    [Test]
    public void not_equal_operator_for_not_equal()
    {
        (new ScrapedResult("<div>text1 text2</div>") != new ScrapedResult("<div>  text1  text3  </div>")).ShouldBe(true);
    }
}