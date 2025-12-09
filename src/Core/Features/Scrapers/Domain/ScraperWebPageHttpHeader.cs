namespace MrWatchdog.Core.Features.Scrapers.Domain;

public record ScraperWebPageHttpHeader
{
    protected ScraperWebPageHttpHeader() {}

    public ScraperWebPageHttpHeader(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; protected set; } = null!;
    public string Value { get; protected set; } = null!;
}