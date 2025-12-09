using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public record ScrapeScraperWebPageCommand(long ScraperId, long ScraperWebPageId) : Command;