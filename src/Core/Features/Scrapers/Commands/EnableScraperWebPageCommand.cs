using CoreBackend.Messages;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public record EnableScraperWebPageCommand(long ScraperId, long ScraperWebPageId) : Command;