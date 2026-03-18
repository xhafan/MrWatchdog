using Reinforced.Typings.Attributes;

namespace CoreBackend.Infrastructure.Rebus;

[TsInterface(IncludeNamespace = false, AutoI = false)]
public class RebusOptions
{
    public string Transport { get; set; } = null!;
    public int MaxDeliveryAttempts { get; set; }
    public int DefaultNumberOfWorkers { get; set; }
    public int? MainNumberOfWorkers { get; set; }
    public int? AdminBulkNumberOfWorkers { get; set; }
    public int? ScrapingNumberOfWorkers { get; set; }
    public int? EmailNumberOfWorkers { get; set; }

    public int GetNumberOfWorkers(string inputQueueName)
    {
        var queueSpecific = inputQueueName switch
        {
            "Main" => MainNumberOfWorkers,
            "AdminBulk" => AdminBulkNumberOfWorkers,
            "Scraping" => ScrapingNumberOfWorkers,
            "Email" => EmailNumberOfWorkers,
            _ => null
        };
        return queueSpecific ?? DefaultNumberOfWorkers;
    }
}
