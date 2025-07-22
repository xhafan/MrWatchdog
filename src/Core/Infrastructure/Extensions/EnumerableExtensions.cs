namespace MrWatchdog.Core.Infrastructure.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> items)
    {
        return items.OfType<T>();
    }
}