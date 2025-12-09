using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public record MakeScraperPublicCommand(long ScraperId) : Command;