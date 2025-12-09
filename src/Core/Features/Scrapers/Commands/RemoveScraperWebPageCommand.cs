using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public record RemoveScraperWebPageCommand(long ScraperId, long ScraperWebPageId) : Command;