namespace MrWatchdog.Core.Features.Scrapers.Domain;

public record ScraperWebPageHttpHeader
{
    protected ScraperWebPageHttpHeader() {}

    public ScraperWebPageHttpHeader(string name, string value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; private set; } = null!;
    public string Value { get; private set; } = null!;
}