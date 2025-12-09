using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public record CreateScraperWebPageCommand(long ScraperId) : Command;