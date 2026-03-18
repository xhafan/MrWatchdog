using Reinforced.Typings.Attributes;

namespace CoreBackend.Infrastructure.Rebus;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public class RebusOptions
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public string Transport { get; set; } = null!;
    public int MaxDeliveryAttempts { get; set; }
    public int DefaultNumberOfWorkers { get; set; }

    // ReSharper disable once CollectionNeverUpdated.Global
    // ReSharper disable once MemberCanBePrivate.Global
    public Dictionary<string, int> NumberOfWorkersByQueue { get; set; } = new();
    // ReSharper restore UnusedAutoPropertyAccessor.Global

    public int GetNumberOfWorkers(string inputQueueName)
    {
        return NumberOfWorkersByQueue.GetValueOrDefault(inputQueueName, DefaultNumberOfWorkers);
    }
}
