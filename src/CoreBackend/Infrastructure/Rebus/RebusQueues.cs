using System.Reflection;

namespace CoreBackend.Infrastructure.Rebus;

public static class RebusQueues
{
    public const string Main = nameof(Main);
    public const string AdminBulk = nameof(AdminBulk);
    public const string Email = nameof(Email);

    private static readonly List<Type> _queueSourceTypes = [typeof(RebusQueues)];

    public static void RegisterQueueSource(Type type) => _queueSourceTypes.Add(type);

    public static readonly Lazy<IReadOnlyCollection<string>> AllQueues = new(() =>
    {
        return _queueSourceTypes
            .SelectMany(type => type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
                .Select(fi => (string)fi.GetRawConstantValue()!))
            .ToArray();
    });
}
