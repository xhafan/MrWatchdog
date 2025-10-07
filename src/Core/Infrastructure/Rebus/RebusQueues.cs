using System.Reflection;

namespace MrWatchdog.Core.Infrastructure.Rebus;

public static class RebusQueues
{
    public const string Main = nameof(Main);
    public const string AdminBulk = nameof(AdminBulk);
    public const string Scraping = nameof(Scraping);

    public static readonly Lazy<IReadOnlyCollection<string>> AllQueues = new(() =>
    {
        return typeof(RebusQueues)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
            .Select(fi => (string) fi.GetRawConstantValue()!)
            .ToArray();
    });
}