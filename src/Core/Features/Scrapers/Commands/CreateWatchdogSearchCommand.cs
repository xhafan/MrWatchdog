using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public record CreateWatchdogSearchCommand(
    long ScraperId, 
    string? SearchTerm
) : Command;