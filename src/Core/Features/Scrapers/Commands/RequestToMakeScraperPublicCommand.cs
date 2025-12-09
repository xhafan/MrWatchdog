using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public record RequestToMakeScraperPublicCommand(long ScraperId) : Command;