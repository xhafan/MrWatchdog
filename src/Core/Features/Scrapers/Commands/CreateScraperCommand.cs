using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public record CreateScraperCommand(long UserId, string Name) : Command;