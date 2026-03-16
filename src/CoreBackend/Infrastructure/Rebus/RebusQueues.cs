using System.Reflection;

namespace CoreBackend.Infrastructure.Rebus;

public static class RebusQueues // todo: move this out of CoreBackend
{
    public const string Main = nameof(Main);
    public const string AdminBulk = nameof(AdminBulk);
    public const string Scraping = nameof(Scraping);
    public const string Email = nameof(Email);

    public static readonly Lazy<IReadOnlyCollection<string>> AllQueues = new(() =>
    {
        return typeof(RebusQueues)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
            .Select(fi => (string) fi.GetRawConstantValue()!)
            .ToArray();
    });
}