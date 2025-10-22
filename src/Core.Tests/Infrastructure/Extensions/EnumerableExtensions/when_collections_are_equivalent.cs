using MrWatchdog.Core.Infrastructure.Extensions;

namespace MrWatchdog.Core.Tests.Infrastructure.Extensions.EnumerableExtensions;

[TestFixture]
public class when_collections_are_equivalent
{
    [Test]
    public void collections_with_items_in_different_order_are_equivalent()
    {
        var a = new[] { 1, 5 };
        var b = new[] { 5, 1 };

        a.AreEquivalent(b).ShouldBe(true);
    }

    [Test]
    public void collection_with_duplicates_are_not_equivalent()
    {
        var a = new[] {1, 5, 5};
        var b = new[] {1, 1, 5};

        a.AreEquivalent(b).ShouldBe(false); 
    }
}