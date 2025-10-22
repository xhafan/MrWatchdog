using CoreUtils.Extensions;

namespace MrWatchdog.Core.Infrastructure.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> items)
    {
        return items.OfType<T>();
    }

    // taken from https://stackoverflow.com/a/3670082/379279 and https://stackoverflow.com/a/3669985/379279
    public static bool AreEquivalent<T>(this IEnumerable<T> items, IEnumerable<T> otherItems)
    {
        var itemList = items.ToList();
        var otherItemList = otherItems.ToList();
        var except = itemList.Except(otherItemList);
        return itemList.Distinct().Count() == otherItemList.Count && except.IsEmpty();
    }
}